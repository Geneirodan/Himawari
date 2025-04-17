using Himawari.Core.Abstractions.Messages;
using Himawari.SillyThings.Options;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.SillyThings.Responses;

public static partial class SS
{
    public sealed record StickerReply(Message Message) : IReply
    {
        public sealed class Handler(Bot bot, IOptionsMonitor<SillyThingsOptions> optionsMonitor)
            : IRequestHandler<StickerReply, Message>
        {
            public async Task<Message> Handle(StickerReply request, CancellationToken cancellationToken)
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