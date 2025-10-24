using System.Text;
using Himawari.Telegram.Application.Resources;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using WTelegram;
using static Himawari.Telegram.Application.Resources.Messages;
using Message = Telegram.Bot.Types.Message;

namespace Himawari.Telegram.Application.Commands;

[BotCommand("/call")]
public sealed record CallAllCommand(Message Message) : ICommand
{
    public sealed class Handler(Bot bot) : IRequestHandler<CallAllCommand, Message>
    {
        public async Task<Message> Handle(CallAllCommand request, CancellationToken cancellationToken)
        {
            var message = request.Message;

            var members = await bot.GetChatMemberList(message.Chat.Id).ConfigureAwait(false);

            var text = members
                .Where(x => !x.User.IsBot)
                .Select(x => x.User.GetDisplayName())
                .Where(x => !string.IsNullOrEmpty(x))
                .Aggregate(
                    new StringBuilder(Calling).Append('\n'),
                    (current, next) => current.Append('•').Append(' ').AppendLine(next)
                )
                .ToString();

            return await bot.SendReplyMessage(message, text).ConfigureAwait(false);
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