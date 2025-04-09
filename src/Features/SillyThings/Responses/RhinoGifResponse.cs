using Himawari.Core.Abstractions;
using Himawari.SillyThings.Options;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.SillyThings.Responses;

public record RhinoGifResponse(Message Message) : IResponse
{
    public sealed class Handler(Bot bot, IOptionsMonitor<SillyThingsOptions> optionsMonitor) : IRequestHandler<RhinoGifResponse, Message>
    {
        public async Task<Message> Handle(RhinoGifResponse request, CancellationToken cancellationToken) =>
            await bot.SendAnimation(request.Message.Chat.Id, optionsMonitor.CurrentValue.RhinoGifUrl).ConfigureAwait(false);
    }
}