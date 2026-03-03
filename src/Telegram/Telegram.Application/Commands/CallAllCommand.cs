using System.Text;
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
using WTelegram;
using static Himawari.Telegram.Application.Resources.Messages;
using Message = Telegram.Bot.Types.Message;

namespace Himawari.Telegram.Application.Commands;

/// <inheritdoc />
[BotCommand("/call")]
public sealed record CallAllCommand(Message Message) : ICommand
{
    public sealed class Handler(Bot bot, IOutgoingTelegramBot outgoingBot, ILogger<Handler> logger) : IRequestHandler<CallAllCommand, Message>
    {
        public async Task<Message> Handle(CallAllCommand request, CancellationToken cancellationToken)
        {
            var message = request.Message;

            try
            {
                var membersList = await bot.GetChatMemberList(message.Chat.Id).ConfigureAwait(false);
                var text = membersList
                    .Where(x => !x.User.IsBot)
                    .Select(x => x.User.GetDisplayName())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Aggregate(
                        new StringBuilder(Calling).Append('\n'),
                        (current, next) => current.Append('•').Append(' ').AppendLine(next)
                    )
                    .ToString();
                return await outgoingBot.SendReplyMessage(message, text, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "GetChatMemberList failed for chat {ChatId}", message.Chat.Id);
                return await outgoingBot.SendReplyMessage(message, "I can't list chat members (I may need admin rights).", cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<CallAllCommand>(aliases.CurrentValue)
    {
        public override string Description => CommandDescriptions.CallAll;
        public override Func<Message, string, ICommand> Factory => (message, _) => new CallAllCommand(message);
    }
}