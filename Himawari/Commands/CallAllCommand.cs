using System.Text;
using Himawari.Abstractions;
using Himawari.Extensions;
using MediatR;
using Telegram.Bot.Types.Enums;
using WTelegram;
using static Himawari.Resources.Messages;
using Message = Telegram.Bot.Types.Message;

namespace Himawari.Commands;

public record CallAllCommand(Message Message) : ICommand
{
    public class Handler(Bot bot) : IRequestHandler<CallAllCommand, Message>
    {
        public async Task<Message> Handle(CallAllCommand request, CancellationToken cancellationToken)
        {
            var message = request.Message;

            var members = await bot.GetChatMemberList(message.Chat.Id);

            var text = members
                .Where(x => !x.User.IsBot)
                .Select(x=>x.User)
                .Aggregate(
                    new StringBuilder(Calling).Append('\n'),
                    (current, next) => current.Append('•').Append(' ').AppendLine(next.GetDisplayName())
                )
                .ToString();

            return await bot.SendReplyMessage(message, text, ParseMode.MarkdownV2);
        }
    }
}