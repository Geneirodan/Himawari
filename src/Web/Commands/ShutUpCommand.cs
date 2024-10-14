using Himawari.Core.Abstractions;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using Random = System.Random;

namespace Himawari.Web.Commands;

public record ShutUpCommand(Message Message) : ICommand
{
    public class Handler(Bot bot) : IRequestHandler<ShutUpCommand, Message?>
    {
        private readonly string[] _gifUrls =
        [
            "https://media1.tenor.com/m/J3c7EArtlHsAAAAd/shut-up-and-listen-cat.gif",
            "https://media1.tenor.com/m/SsXEN-yHFqgAAAAC/exploding-car-explode.gif",
            "https://media1.tenor.com/m/RQEIzNTFnR4AAAAd/shut-up-%D1%80%D0%BE%D1%82%D0%B8%D0%BA-%D0%BE%D1%84%D1%84.gif"
        ];

        public async Task<Message?> Handle(ShutUpCommand request, CancellationToken cancellationToken)
        {
            var message = request.Message;
            var parameters = message.ReplyToMessage is { } reply
                ? new ReplyParameters
                {
                    ChatId = reply.Chat.Id,
                    MessageId = reply.MessageId
                }
                : new ReplyParameters
                {
                    ChatId = message.Chat.Id,
                    MessageId = message.MessageId
                };
            var index = Random.Shared.Next(_gifUrls.Length);
            return await bot.SendAnimation(
                chatId: message.Chat.Id,
                animation: _gifUrls[index],
                replyParameters: parameters);
        }
    }
}