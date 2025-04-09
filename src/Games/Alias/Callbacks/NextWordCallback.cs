using Himawari.Alias.Services;
using Himawari.Core.Abstractions;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

public sealed record NextWordCallback(CallbackQuery Query) : AbstractCallback(Query)
{
    public sealed class Handler(Bot bot, IAliasService service, ISender sender) : IRequestHandler<NextWordCallback>
    {
        public async Task Handle(NextWordCallback request, CancellationToken cancellationToken)
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
                service.ResetWord(chatId);
                var req = new SeeWordCallback(request.Query);
                await sender.Send(req, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}