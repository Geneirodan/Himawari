using System.Globalization;
using System.Text.Encodings.Web;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Localization;
using Himawari.Telegram.Core.RateLimiting;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types.Enums;
using WTelegram;
using WTelegram.Types;

namespace Himawari.Telegram.Application;

/// <summary>
/// Handles callback queries from <see cref="Core.Keyboards.UserPickerKeyboard"/> for the "who" command.
/// Callback data format: <c>who:{userId}|{label}</c>. Edits the original message to show the selected presenter and removes the keyboard.
/// Resolves culture from chat /lang first (via scoped <see cref="IExplicitLanguageResolver"/>), then falls back to Telegram LanguageCode.
/// </summary>
[PublicAPI]
public sealed class WhoCallbackHandler(
    Bot bot,
    IOutgoingTelegramBot outgoingBot,
    IBotLocalizer loc,
    IServiceScopeFactory scopeFactory,
    ICultureResolver cultureResolver) : IUpdateHandler
{
    private const string Prefix = "who:";

    /// <inheritdoc />
    public async Task OnUpdate(Update arg)
    {
        if (arg is not { Type: UpdateType.CallbackQuery, CallbackQuery: { } query })
            return;
        if (query.Data is not { } data || !data.StartsWith(Prefix, StringComparison.Ordinal))
            return;

        // Split by first "|" only so label may contain "|" (e.g. "Maria | S.").
        var separatorIndex = data.IndexOf('|');
        var label = separatorIndex >= 0
            ? data[(separatorIndex + 1)..]
            : data[Prefix.Length..];
        if (string.IsNullOrEmpty(label))
            label = "?";

        var chatId = query.Message?.Chat?.Id ?? 0;
        var messageId = query.Message?.MessageId ?? 0;
        if (chatId == 0 || messageId == 0)
            return;

        CultureInfo culture;
        using (var scope = scopeFactory.CreateScope())
        {
            var explicitResolver = scope.ServiceProvider.GetRequiredService<IExplicitLanguageResolver>();
            culture = await explicitResolver.ResolveAsync(chatId, cancellationToken: default).ConfigureAwait(false)
                ?? cultureResolver.Resolve(query.From?.LanguageCode);
        }

        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        var displayName = HtmlEncoder.Default.Encode(label);
        var text = loc[Loc.WhoPickResult, displayName];

        await outgoingBot.EditMessageTextAsync(chatId, messageId, text, ParseMode.Html, replyMarkup: null).ConfigureAwait(false);
        await bot.AnswerCallbackQuery(query.Id).ConfigureAwait(false);
    }
}
