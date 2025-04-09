using Himawari.Core.Abstractions;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.SillyThings.Responses;

public static partial class SS
{
    public sealed record DetectedResponse(Message Message) : IResponse
    {
        public sealed class Handler(Bot bot) : IRequestHandler<DetectedResponse, Message>
        {
            public async Task<Message> Handle(DetectedResponse request, CancellationToken cancellationToken) =>
                await bot.SendMessage(
                    chatId: request.Message.Chat.Id,
                    text: "\u26a1\ufe0f SS detected! \u26a1\ufe0f",
                    replyParameters: new ReplyParameters
                    {
                        MessageId = request.Message.MessageId,
                        ChatId = request.Message.Chat.Id,
                        Quote = "SS"
                    }
                ).ConfigureAwait(false);
        }
    }
}