using Himawari.SillyThings.Options;
using Himawari.Telegram.Core.Abstractions.Messages;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.SillyThings.Responses;

public sealed record SSDetectedReply(Message Message) : IReply
{
    public sealed class Handler(Bot bot, IOptionsMonitor<SillyThingsOptions> optionsMonitor)
        : IRequestHandler<SSDetectedReply, IEnumerable<Message>>
    {
        public async Task<IEnumerable<Message>> Handle(SSDetectedReply request, CancellationToken cancellationToken)
        {
            var message = request.Message;
            return
            [
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "\u26a1\ufe0f SS detected! \u26a1\ufe0f",
                    replyParameters: new ReplyParameters
                    {
                        MessageId = message.MessageId,
                        ChatId = message.Chat.Id,
                        Quote = "SS"
                    }
                ).ConfigureAwait(false),
                await bot.SendSticker(
                    chatId: message.Chat.Id,
                    sticker: optionsMonitor.CurrentValue.SsStickerUrl,
                    replyParameters: new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat.Id }
                ).ConfigureAwait(false)
            ];
        }
    }
}