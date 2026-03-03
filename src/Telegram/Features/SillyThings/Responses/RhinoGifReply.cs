using Himawari.SillyThings.Options;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.RateLimiting;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Himawari.SillyThings.Responses;

/// <summary>Reply when the rhino trigger word is detected: sends the rhino GIF from <see cref="SillyThingsOptions.RhinoGifUrl"/>.</summary>
public sealed record RhinoGifReply(Message Message) : IReply
{
    /// <inheritdoc />
    public sealed class Handler(IOutgoingTelegramBot bot, IOptionsMonitor<SillyThingsOptions> optionsMonitor)
        : IRequestHandler<RhinoGifReply, IEnumerable<Message>>
    {
        public async Task<IEnumerable<Message>> Handle(RhinoGifReply request, CancellationToken cancellationToken)
        {
            var url = optionsMonitor.CurrentValue.RhinoGifUrl;
            if (string.IsNullOrEmpty(url))
                return [];
            return
            [
                await bot.SendAnimationAsync(
                    request.Message.Chat.Id,
                    url,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false)
            ];
        }
    }
}