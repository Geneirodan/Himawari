using Himawari.Core.Abstractions;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

public record SeeWordCallback(CallbackQuery Query) : ICallback
{
    public class Handler(Bot bot, IAliasService service) : IRequestHandler<SeeWordCallback>
    {
        public async Task Handle(SeeWordCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message is null)
                return;
            var chatId = request.Query.Message.Chat.Id;
            if (service.GetPresenterId(chatId) is not { } presenterId)
                await bot.AnswerCallbackQuery(request.Query.Id, GameIsNotStarted, true);
            else if (presenterId != request.Query.From.Id)
                await bot.AnswerCallbackQuery(request.Query.Id, Forbidden, true);
            else
            {
                var word = await service.GetCurrentWordAsync(chatId, cancellationToken);
                await bot.AnswerCallbackQuery(request.Query.Id, word, true);
            }
        }
    }
}