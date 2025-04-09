using Himawari.Core.Abstractions;
using Himawari.SillyThings.Options;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.SillyThings.Responses;

public static partial class SS
{
    public sealed record StickerResponse(Message Message) : IResponse
    {
        public sealed class Handler(Bot bot, IOptionsMonitor<SillyThingsOptions> optionsMonitor) : IRequestHandler<StickerResponse, Message>
        {
            public async Task<Message> Handle(StickerResponse request, CancellationToken cancellationToken)
            {
                var message = request.Message;
                return await bot.SendSticker(
                    chatId: message.Chat.Id,
                    sticker: optionsMonitor.CurrentValue.SsGifUrl,
                    replyParameters: new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat.Id }
                ).ConfigureAwait(false);
            }
        }
    }
}