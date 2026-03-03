using Himawari.SillyThings.Options;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.RateLimiting;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.SillyThings.Responses;

/// <summary>Reply when "SS" is detected in the message: sends a lightning message and the SS sticker from <see cref="SillyThingsOptions.SsStickerUrl"/>.</summary>
public sealed record SSDetectedReply(Message Message) : IReply
{
    /// <inheritdoc />
    public sealed class Handler(IOutgoingTelegramBot bot, IOptionsMonitor<SillyThingsOptions> optionsMonitor)
        : IRequestHandler<SSDetectedReply, IEnumerable<Message>>
    {
        public async Task<IEnumerable<Message>> Handle(SSDetectedReply request, CancellationToken cancellationToken)
        {
            var message = request.Message;
            var replyParams = new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat.Id, Quote = "SS" };
            var list = new List<Message>
            {
                await bot.SendReplyAsync(
                    message,
                    "\u26a1\ufe0f SS detected! \u26a1\ufe0f",
                    ParseMode.MarkdownV2,
                    null,
                    replyParams,
                    cancellationToken
                ).ConfigureAwait(false)
            };
            var stickerUrl = optionsMonitor.CurrentValue.SsStickerUrl;
            if (!string.IsNullOrEmpty(stickerUrl))
            {
                list.Add(await bot.SendStickerAsync(
                    message.Chat.Id,
                    stickerUrl,
                    new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat.Id },
                    cancellationToken
                ).ConfigureAwait(false));
            }
            return list;
        }
    }
}