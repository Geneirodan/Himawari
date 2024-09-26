using System.Text;
using Himawari.Resources;
using Himawari.Services;
using MediatR;
using Microsoft.Extensions.Localization;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Commands;

public record CallAllCommand(Message Message, CommandInfo CommandInfo) : ICommand
{
    public class Handler(Bot bot) : IRequestHandler<CallAllCommand, Message>
    {
        public async Task<Message> Handle(CallAllCommand request, CancellationToken cancellationToken)
        {
            var (message, (_, _, cultureInfo)) = request;
            
            var callingString = Messages.ResourceManager.GetString(nameof(Messages.Calling), cultureInfo);
            var builder = new StringBuilder(callingString).Append('\n');
            var members = await bot.GetChatMemberList(message.Chat.Id);

            builder = members
                .Select(x => x.User.Username)
                .Aggregate(builder, (current, next) => current.Append('@').Append(next).Append('\n'));

            return await bot.SendTextMessage(
                chatId: message.Chat.Id,
                text: builder.ToString(),
                replyParameters: new ReplyParameters
                {
                    MessageId = message.MessageId,
                    ChatId = message.Chat.Id
                }
            );
        }
    }
}