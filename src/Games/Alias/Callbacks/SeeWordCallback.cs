using Himawari.Alias.Services;
using Himawari.Core.Abstractions;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

public record SeeWordCallback(CallbackQuery Query) : ICallback
{
    public sealed class Handler(Bot bot, IAliasService service) : IRequestHandler<SeeWordCallback>
    {
        public async Task Handle(SeeWordCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message is null)
                return;
            var chatId = request.Query.Message.Chat.Id;
            if (service.GetPresenterId(chatId) is not { } presenterId)
                await bot.AnswerCallbackQuery(request.Query.Id, GameIsNotStarted, true).ConfigureAwait(false);
            else if (presenterId != request.Query.From.Id)
                await bot.AnswerCallbackQuery(request.Query.Id, Forbidden, true).ConfigureAwait(false);
            else
            {
                var word = await service.GetCurrentWordAsync(chatId, cancellationToken).ConfigureAwait(false);
                await bot.AnswerCallbackQuery(request.Query.Id, word, true).ConfigureAwait(false);
            }
        }
    }
}