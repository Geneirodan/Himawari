using Himawari.Core.Abstractions;
using Himawari.Core.Extensions;
using Himawari.SpellChecking.Resources;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WTelegram;

namespace Himawari.SpellChecking;

public record SendCorrectedTextMessage(Message Message, string Text) : ICommand 
{
    public class Handler(Bot bot) : IRequestHandler<SendCorrectedTextMessage, Message>
    {
        public Task<Message> Handle(SendCorrectedTextMessage request, CancellationToken cancellationToken)
        {
            var (message, text) = request;
            return bot.SendReplyMessage(message, $"{Messages.Maybe}\n**>{text}", ParseMode.MarkdownV2);
        }
    }
}