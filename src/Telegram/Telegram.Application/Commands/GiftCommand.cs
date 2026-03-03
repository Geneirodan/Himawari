using Himawari.Telegram.Application.Resources;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.RateLimiting;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;
using static System.StringComparison;

namespace Himawari.Telegram.Application.Commands;

/// <inheritdoc />
[BotCommand("/gift")]
public sealed record GiftCommand(Message Message, string Rest) : ICommand
{
    public sealed class Handler(Bot bot, IOutgoingTelegramBot outgoingBot, ILogger<Handler> logger) : IRequestHandler<GiftCommand, Message>
    {
        public async Task<Message> Handle(GiftCommand request, CancellationToken cancellationToken)
        {
            var (message, rest) = request;
            var arr = rest.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length == 0)
                return await outgoingBot.SendReplyMessage(message, Messages.NotUnderstandGift, cancellationToken: cancellationToken).ConfigureAwait(false);

            IReadOnlyList<ChatMember> members;
            try
            {
                members = await bot.GetChatMemberList(message.Chat.Id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "GetChatMemberList failed for chat {ChatId}", message.Chat.Id);
                return await outgoingBot.SendReplyMessage(message, "I can't list chat members (I may need admin rights).", cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var username = members
                .FirstOrDefault(x => string.Equals(x.User.Username, arr[0].TrimStart('@'), OrdinalIgnoreCase))?
                .User.Username;
            var text = username switch
            {
                null => Messages.MemberNotFound,
                not null when arr.Length == 1 => Messages.GiftNotFound,
                _ => $"{string.Format(Messages.Gift, $"@{message.From?.Username}", $"@{username}")} {arr[1]}"
            };

            return await outgoingBot.SendReplyMessage(message, text, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<GiftCommand>(aliases.CurrentValue)
    {
        public override string Description => CommandDescriptions.Gift;
        public override Func<Message, string, ICommand> Factory => (message, rest) => new GiftCommand(message, rest);
    }
}