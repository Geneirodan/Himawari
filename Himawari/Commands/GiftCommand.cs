using Himawari.Abstractions;
using Himawari.Extensions;
using Himawari.Resources;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Commands;

public record GiftCommand(Message Message, string Rest) : ICommand
{
    public class Handler(Bot bot) : IRequestHandler<GiftCommand, Message>
    {
        public async Task<Message> Handle(GiftCommand request, CancellationToken cancellationToken)
        {
            string? text;
            var (message, rest) = request;
            var arr = rest.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 0)
                text = Messages.NotUnderstandGift;
            else
            {
                var members = await bot.GetChatMemberList(message.Chat.Id);
                var member = members.FirstOrDefault(x => x.User.Username == arr[0].TrimStart('@'));
                if (member is null)
                    text = Messages.MemberNotFound;
                else if (arr.Length == 1)
                    text = Messages.GiftNotFound;
                else
                {
                    var start = Messages.Gift;
                    start = string.Format(start, $"@{message.From?.Username}", $"@{member.User.Username}");
                    text = $"{start} {arr[1]}";
                }
            }

            return await bot.SendReplyMessage(message, text);
        }
    }
}