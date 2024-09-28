using Himawari.Abstractions;
using Himawari.Abstractions.Services;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Resources.Games.Alias;

namespace Himawari.Games.Alias.Callbacks;

public record EndGameCallback(CallbackQuery Query) : ICallback<Message?>
{
    public class Handler(Bot bot, IAliasService service) : IRequestHandler<EndGameCallback, Message?>
    {
        public async Task<Message?> Handle(EndGameCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message is not { } message)
                return null;

            var chatId = message.Chat.Id;
            service.Restart(chatId);
            // await bot.DeleteMessages(message.Chat.Id, service.Messages.ToArray()).ConfigureAwait(false);
            return await bot.SendTextMessage(chatId, GameEnded).ConfigureAwait(false);
        }
    }
}