using Himawari.Core.Abstractions;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

public record EndGameCallback(CallbackQuery Query) : ICallback<Message?>
{
    public class Handler(Bot bot, IAliasService service) : IRequestHandler<EndGameCallback, Message?>
    {
        public async Task<Message?> Handle(EndGameCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message is not { } message)
                return null;

            var chatId = message.Chat.Id;
            if (service.GetPresenterId(chatId) is null)
            {
                await bot.AnswerCallbackQuery(request.Query.Id, GameIsNotStarted, true);
                return null;
            }

            service.Restart(chatId);
            // await bot.DeleteMessages(message.Chat.Id, service.Messages.ToArray());
            return await bot.SendTextMessage(chatId, GameEnded);
        }
    }
}