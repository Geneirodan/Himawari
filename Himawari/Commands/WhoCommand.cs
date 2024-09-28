using System.Collections;
using System.Globalization;
using System.Text;
using Himawari.Abstractions;
using Himawari.Extensions;
using Himawari.Resources.Commands;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Commands;

public record WhoCommand(Message Message, string Rest) : ICommand
{
    // ReSharper disable once UnusedType.Global
    public class Handler(Bot bot) : IRequestHandler<WhoCommand, Message>
    {
        public async Task<Message> Handle(WhoCommand request, CancellationToken cancellationToken)
        {
            var (message, rest) = request;

            var members = (await bot.GetChatMemberList(message.Chat.Id)).Where(x => !x.User.IsBot).ToArray();

            var index = Random.Shared.Next(members.Length);

            var cultureInfo = CultureInfo.CurrentUICulture;
            var resourceSet = Who.ResourceManager
                .GetResourceSet(cultureInfo, true, true)!
                .Cast<DictionaryEntry>()
                .Select(entry => entry.Key)
                .Cast<string>()
                .ToArray();

            var quote = Random.Shared.Next(resourceSet.Length);

            var text = Who.ResourceManager.GetString(resourceSet[quote], cultureInfo);
            var stringBuilder = new StringBuilder(text)
                .Append(' ')
                .Append(members[index].User.GetUsername());

            if (!string.IsNullOrWhiteSpace(rest))
                stringBuilder.Append(' ').Append(rest.TrimEnd('?'));

            return await bot.SendReplyMessage(message, stringBuilder.ToString());
        }
    }
}