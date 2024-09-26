using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Commands;

public record SecretCommand(Message Message) : IRequest
{
    public class Handler(Bot bot) : IRequestHandler<SecretCommand>
    {
        public async Task Handle(SecretCommand request, CancellationToken cancellationToken)
        {
            await bot.SendTextMessage(
                request.Message.Chat.Id,
                "\u26a1\ufe0fSS detected!\u26a1\ufe0f",
                replyParameters: new ReplyParameters
                {
                    MessageId = request.Message.MessageId,
                    ChatId = request.Message.Chat.Id,
                    Quote = "SS"
                }
            );
            await bot.SendSticker(
                request.Message.Chat.Id,
                "https://sl.combot.org/nazi_anime/webp/58xf09f87a9f09f87aa.webp",
                replyParameters: new ReplyParameters
                {
                    MessageId = request.Message.MessageId,
                    ChatId = request.Message.Chat.Id
                }
            );
        }
    }
}