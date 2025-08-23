using Himawari.Core.Abstractions.Messages;
using Himawari.Core.Extensions;
using Himawari.SpellChecking.Resources;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.SpellChecking.Replies;

public sealed record SendCorrectedTextMessageReply(Message Message, string Text) : IReply
{
    public sealed class Handler(Bot bot) : IRequestHandler<SendCorrectedTextMessageReply, IEnumerable<Message>>
    {
        public async Task<IEnumerable<Message>> Handle(SendCorrectedTextMessageReply request, CancellationToken cancellationToken)
        {
            var (message, text) = request;
            return [await bot.SendReplyMessage(message, $"{Messages.Maybe}\n**>{text}").ConfigureAwait(false)];
        }
    }
}