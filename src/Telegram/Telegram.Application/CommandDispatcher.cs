using System.Globalization;
using System.Text.RegularExpressions;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Html;
using Himawari.Telegram.Core.Localization;
using Himawari.Telegram.Core.Options;
using Himawari.Telegram.Core.RateLimiting;
using Himawari.Telegram.Core.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTelegram;
using Message = Telegram.Bot.Types.Message;
using ChatType = Telegram.Bot.Types.Enums.ChatType;
using ParseMode = Telegram.Bot.Types.Enums.ParseMode;

namespace Himawari.Telegram.Application;

/// <summary>
/// First-pass message handler: parses slash commands and natural-language triggers (no leading slash),
/// resolves them via <see cref="ICommandResolver.Resolve"/> (exact, alias, fuzzy), and sends the corresponding
/// <see cref="ICommand"/> to MediatR or "Did you mean?" suggestion.
/// </summary>
[PublicAPI]
public sealed class CommandDispatcher(
    Bot bot,
    IBotIdentity botIdentity,
    ICommandResolver resolver,
    ITokenizerCache tokenizerCache,
    IOutgoingTelegramBot outgoingBot,
    IServiceProvider serviceProvider,
    IOptions<CommandRegistryOptions> registryOptions,
    ILogger<CommandDispatcher> logger)
    : AbstractDispatcher
{
    /// <inheritdoc />
    protected override async Task OnNewMessage(Message msg)
    {
        if (msg.Text is not { } messageText) return;

        logger.LogDebug("CommandDispatcher received: ChatId={ChatId} Text={Text}", msg.Chat.Id, messageText);

        var scope = serviceProvider.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var (ok, commandWithoutSlash, restText) = await TryGetCommandAndRestAsync(msg, messageText, bot, botIdentity, resolver, tokenizerCache, registryOptions.Value).ConfigureAwait(false);
            if (!ok || string.IsNullOrEmpty(commandWithoutSlash))
                return;

            var layoutCorrector = scope.ServiceProvider.GetService<ICommandLayoutCorrector>();
            if (layoutCorrector is not null && layoutCorrector.TryCorrect(commandWithoutSlash, out var corrected) && !string.IsNullOrEmpty(corrected))
                commandWithoutSlash = corrected;

            var result = resolver.Resolve(commandWithoutSlash.AsSpan());

            switch (result.Kind)
            {
                case CommandMatchKind.Exact:
                case CommandMatchKind.Alias:
                    var req = result.Entry!.Factory(msg, restText);
                    await sender.Send(req).ConfigureAwait(false);
                    break;
                case CommandMatchKind.Fuzzy when registryOptions.Value.ShowSuggestions && result.Suggestions is { Length: > 0 } suggestions:
                    var list = string.Join(", ", suggestions.Select(s => $"<code>/{s}</code>"));
                    var explicitResolver = scope.ServiceProvider.GetRequiredService<IExplicitLanguageResolver>();
                    var cultureResolver = scope.ServiceProvider.GetRequiredService<ICultureResolver>();
                    var culture = await explicitResolver.ResolveAsync(msg.Chat.Id, cancellationToken: default).ConfigureAwait(false)
                        ?? cultureResolver.Resolve(msg.From?.LanguageCode);
                    Thread.CurrentThread.CurrentUICulture = culture;
                    Thread.CurrentThread.CurrentCulture = culture;
                    var loc = scope.ServiceProvider.GetRequiredService<IBotLocalizer>();
                    await outgoingBot.SendReplyMessage(msg, loc[Loc.UnknownCommandDidYouMean, list], ParseMode.Html).ConfigureAwait(false);
                    break;
                case CommandMatchKind.Fuzzy:
                case CommandMatchKind.NotFound:
                    break;
            }
        }
    }

    private static async Task<(bool Ok, string CommandWithoutSlash, string RestText)> TryGetCommandAndRestAsync(
        Message msg,
        string messageText,
        Bot bot,
        IBotIdentity botIdentity,
        ICommandResolver resolver,
        ITokenizerCache tokenizerCache,
        CommandRegistryOptions options)
    {
        var needBotUsername = (messageText.StartsWith('/') && messageText.Length > 1 && messageText.Contains('@'))
            || (msg.Chat.Type != ChatType.Private && !options.NaturalLanguageInPrivateChatsOnly);
        var botUsername = needBotUsername ? await botIdentity.GetUsernameAsync().ConfigureAwait(false) : null;

        if (messageText.StartsWith('/') && messageText.Length > 1)
        {
            var (command, restText, forMe) = await bot.ParseCommandAsync(messageText, botUsername).ConfigureAwait(false);
            if (!forMe) return (false, string.Empty, string.Empty);
            return (true, command.TrimStart('/').Trim(), restText);
        }
        string textForNl;
        if (msg.Chat.Type == ChatType.Private)
            textForNl = messageText;
        else if (options.NaturalLanguageInPrivateChatsOnly)
            return (false, string.Empty, string.Empty);
        else
        {
            if (string.IsNullOrEmpty(botUsername) || !TryStripBotMention(messageText, botUsername, out textForNl))
                return (false, string.Empty, string.Empty);
        }
        return TryResolveNaturalLanguage(textForNl, resolver, tokenizerCache, out var cmd, out var r)
            ? (true, cmd, r)
            : (false, string.Empty, string.Empty);
    }

    /// <summary>When message starts with @username (case-insensitive), strips it and outputs the remainder.</summary>
    private static bool TryStripBotMention(string messageText, string botUsername, out string textWithoutMention)
    {
        textWithoutMention = string.Empty;
        var trimmed = messageText.Trim();
        if (trimmed.Length == 0 || trimmed[0] != '@')
            return false;
        var space = trimmed.IndexOf(' ');
        var mention = space < 0 ? trimmed : trimmed[..space];
        if (mention.Length != 1 + botUsername.Length ||
            !string.Equals(mention[1..], botUsername, StringComparison.OrdinalIgnoreCase))
            return false;
        textWithoutMention = (space < 0 ? string.Empty : trimmed[(space + 1)..]).Trim();
        return true;
    }

    /// <summary>
    /// Matches message text against natural-language trigger regex patterns (whole-word, case-insensitive when pattern uses (?i)).
    /// First matching pattern wins; <paramref name="rest"/> is the text after the match.
    /// </summary>
    private static bool TryResolveNaturalLanguage(
        string messageText,
        ICommandResolver resolver,
        ITokenizerCache tokenizerCache,
        out string commandWithoutSlash,
        out string rest)
    {
        commandWithoutSlash = string.Empty;
        rest = string.Empty;
        var lookup = resolver.GetAlternateLookup();
        if (lookup.Count == 0)
            return false;
        var trimmedFull = messageText.Trim();
        if (trimmedFull.Length == 0)
            return false;
        foreach (var kv in lookup)
        {
            var match = Regex.Match(trimmedFull, kv.Key, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(500));
            if (!match.Success)
                continue;
            commandWithoutSlash = kv.Value;
            rest = trimmedFull[(match.Index + match.Length)..].Trim();
            return true;
        }
        return false;
    }
}