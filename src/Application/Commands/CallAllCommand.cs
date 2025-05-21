using System.Text;
using Himawari.Application.Resources;
using Himawari.Core.Abstractions;
using Himawari.Core.Abstractions.Messages;
using Himawari.Core.Attributes;
using Himawari.Core.Extensions;
using Himawari.Core.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using WTelegram;
using static Himawari.Application.Resources.Messages;
using Message = Telegram.Bot.Types.Message;

namespace Himawari.Application.Commands;

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
                .Select(x => x.User)
                .Aggregate(
                    new StringBuilder(Calling).Append('\n'),
                    (current, next) => current.Append('•').Append(' ').AppendLine(next.GetDisplayName())
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