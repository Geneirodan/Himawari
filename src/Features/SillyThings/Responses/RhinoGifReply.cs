using Himawari.Core.Abstractions.Messages;
using Himawari.SillyThings.Options;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.SillyThings.Responses;

public sealed record RhinoGifReply(Message Message) : IReply
{
    public sealed class Handler(Bot bot, IOptionsMonitor<SillyThingsOptions> optionsMonitor)
        : IRequestHandler<RhinoGifReply, IEnumerable<Message>>
    {
        public async Task<IEnumerable<Message>> Handle(RhinoGifReply request, CancellationToken cancellationToken) =>
        [
            await bot.SendAnimation(request.Message.Chat.Id, optionsMonitor.CurrentValue.RhinoGifUrl)
                .ConfigureAwait(false)
        ];
    }
}