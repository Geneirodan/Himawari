using Himawari.Core.Abstractions;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

public record NextWordCallback(CallbackQuery Query) : ICallback
{
    public class Handler(Bot bot, IAliasService service, ISender sender) : IRequestHandler<NextWordCallback>
    {
        public async Task Handle(NextWordCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message == null)
                return;

            var chatId = request.Query.Message.Chat.Id;
            if (service.GetPresenterId(chatId) is not { } presenterId)
                await bot.AnswerCallbackQuery(request.Query.Id, GameIsNotStarted, true);
            else if (presenterId != request.Query.From.Id)
                await bot.AnswerCallbackQuery(request.Query.Id, Forbidden, true);
            else
            {
                service.ResetWord(chatId);
                var req = new SeeWordCallback(request.Query);
                await sender.Send(req, cancellationToken);
            }
        }
    }
}