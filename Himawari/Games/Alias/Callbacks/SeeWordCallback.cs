using Himawari.Abstractions;
using Himawari.Abstractions.Services;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Resources.Games.Alias;

namespace Himawari.Games.Alias.Callbacks;

public record SeeWordCallback(CallbackQuery Query) : ICallback
{
    public class Handler(Bot bot, IAliasService service) : IRequestHandler<SeeWordCallback>
    {
        public async Task Handle(SeeWordCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message == null)
                return;
            var chatId = request.Query.Message.Chat.Id;
            if (service.GetPresenterId(chatId) != request.Query.From.Id)
                await bot.AnswerCallbackQuery(request.Query.Id, Forbidden, true).ConfigureAwait(false);
            else
            {
                var word = await service.GetCurrentWordAsync(chatId, cancellationToken).ConfigureAwait(false);
                await bot.AnswerCallbackQuery(request.Query.Id, word, true).ConfigureAwait(false);
            }
        }
    }
}