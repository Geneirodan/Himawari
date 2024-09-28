using Himawari.Abstractions;
using Himawari.Abstractions.Services;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Resources.Games.Alias;

namespace Himawari.Games.Alias.Callbacks;

public record NextWordCallback(CallbackQuery Query) : ICallback
{
    public class Handler(Bot bot, IAliasService service, ISender sender) : IRequestHandler<NextWordCallback>
    {
        public async Task Handle(NextWordCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message == null)
                return;

            var chatId = request.Query.Message.Chat.Id;
            if (service.GetPresenterId(chatId) != request.Query.From.Id)
                await bot.AnswerCallbackQuery(request.Query.Id, Forbidden, true).ConfigureAwait(false);
            else
            {
                service.ResetWord(chatId);
                var req = new SeeWordCallback(request.Query);
                await sender.Send(req, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}