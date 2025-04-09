using Himawari.Core.Abstractions;
using Himawari.Core.Extensions;
using Himawari.SpellChecking.Resources;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WTelegram;

namespace Himawari.SpellChecking.Responses;

public sealed record SendCorrectedTextMessageResponse(Message Message, string Text) : IResponse 
{
    public sealed class Handler(Bot bot) : IRequestHandler<SendCorrectedTextMessageResponse, Message>
    {
        public Task<Message> Handle(SendCorrectedTextMessageResponse request, CancellationToken cancellationToken)
        {
            var (message, text) = request;
            return bot.SendReplyMessage(message, $"{Messages.Maybe}\n**>{text}", ParseMode.MarkdownV2);
        }
    }
}