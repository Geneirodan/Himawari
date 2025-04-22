using Himawari.Core.Abstractions.Messages;
using Himawari.Core.Extensions;
using Himawari.SpellChecking.Resources;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.SpellChecking.Responses;

public sealed record SendCorrectedTextMessageReply(Message Message, string Text) : IReply
{
    public sealed class Handler(Bot bot) : IRequestHandler<SendCorrectedTextMessageReply, Message>
    {
        public Task<Message> Handle(SendCorrectedTextMessageReply request, CancellationToken cancellationToken)
        {
            var (message, text) = request;
            return bot.SendReplyMessage(message, $"{Messages.Maybe}\n**>{text}");
        }
    }
}