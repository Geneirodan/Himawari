using Himawari.Telegram.Application.Resources;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Localization;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.Options;
using Himawari.Telegram.Core.RateLimiting;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.Telegram.Application.Commands;

/// <inheritdoc />
[BotCommand("/lang")]
public sealed record LangCommand(Message Message, string Rest) : ICommand
{
    public sealed class Handler(
        IOutgoingTelegramBot bot,
        ILanguageRepository languageRepo,
        IOptions<BotOptions> botOptions,
        IBotLocalizer loc) : IRequestHandler<LangCommand, Message>
    {
        public async Task<Message> Handle(LangCommand request, CancellationToken cancellationToken)
        {
            var message = request.Message;
            var code = request.Rest?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToLowerInvariant();
            var supported = botOptions.Value.SupportedLocales;

            if (code is null || !supported.Contains(code, StringComparer.OrdinalIgnoreCase))
            {
                var list = string.Join(", ", supported.Select(s => $"<code>{s}</code>"));
                return await bot.SendReplyMessage(
                    message,
                    loc[Loc.LanguageNotFound, list],
                    ParseMode.Html,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            await languageRepo.SetAsync(message.Chat.Id, code, cancellationToken).ConfigureAwait(false);

            return await bot.SendReplyMessage(
                message,
                loc[Loc.LanguageSet, code],
                ParseMode.Html,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<LangCommand>(aliases.CurrentValue)
    {
        public override string Description => CommandDescriptions.Lang;
        public override Func<Message, string, ICommand> Factory => (message, rest) => new LangCommand(message, rest);
    }
}
