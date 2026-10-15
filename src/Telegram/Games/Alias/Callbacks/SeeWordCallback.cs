using Himawari.Alias.Services;
using Himawari.Telegram.Core.Abstractions;
using Mediator;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

public sealed record SeeWordCallback(CallbackQuery Query) : AbstractCallback(Query)
{
    public sealed class Handler(Bot bot, IAliasService service) : IRequestHandler<SeeWordCallback>
    {
        public async ValueTask<Unit> Handle(SeeWordCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message?.Chat.Id is not { } chatId)
                return Unit.Value;
            var word =
                service.GetPresenterId(chatId) is not { } presenterId ? GameIsNotStarted
                : presenterId != request.Query.From.Id ? Forbidden
                : await service.GetOrCreateCurrentWordAsync(chatId, cancellationToken).ConfigureAwait(false);
            await bot.AnswerCallbackQuery(request.Query.Id, word, showAlert: true).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
