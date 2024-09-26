using System.Collections;
using System.Text;
using Himawari.Resources.Commands;
using Himawari.Services;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Commands;

public record WhoCommand(Message Message, CommandInfo CommandInfo, string Rest) : ICommand
{
    public class Handler(Bot bot) : IRequestHandler<WhoCommand, Message>
    {
        public async Task<Message> Handle(WhoCommand request, CancellationToken cancellationToken)
        {
            var (message, (_, _, cultureInfo), rest) = request;
            
            var members = await bot.GetChatMemberList(message.Chat.Id);
            
            var index = Random.Shared.Next(members.Length);
            
            var resourceSet = Who.ResourceManager
                .GetResourceSet(cultureInfo, true, true)!
                .Cast<DictionaryEntry>()
                .Select(entry => entry.Key)
                .Cast<string>()
                .ToArray();

            var quote = Random.Shared.Next(resourceSet.Length);
            
            var text = Who.ResourceManager.GetString(resourceSet[quote], cultureInfo);
            var stringBuilder = new StringBuilder(text).Append(' ').Append('@').Append(members[index].User.Username);
            
            if (!string.IsNullOrWhiteSpace(rest))
                stringBuilder.Append(' ').Append(rest.TrimEnd('?'));
            
            return await bot.SendTextMessage(
                chatId: message.Chat.Id,
                text: stringBuilder.ToString(),
                replyParameters: new ReplyParameters
                {
                    MessageId = message.MessageId,
                    ChatId = message.Chat.Id
                }
            );
        }
    }
}