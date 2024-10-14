using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Web.Commands;

public record PoopMessage(Message Message) : IRequest
{
    public class Handler(Bot bot) : IRequestHandler<PoopMessage>
    {
        public async Task Handle(PoopMessage request, CancellationToken cancellationToken)
        {
            await bot.SendAnimation(
                request.Message.Chat.Id,
                "https://media1.tenor.com/m/1Nfz7t6LXG8AAAAd/hippo-crap.gif"
            );
        }
    }
}