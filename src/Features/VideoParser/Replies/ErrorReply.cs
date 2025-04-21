using Himawari.Core.Abstractions.Messages;
using Himawari.Core.Extensions;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.VideoParser.Replies;

public record ErrorReply(Message Message, string Type) : IReply
{
    public class Handler(Bot bot) : IRequestHandler<ErrorReply, Message>
    {
        public async Task<Message> Handle(ErrorReply request, CancellationToken cancellationToken)
        {
            var (message, type) = request;
            return await bot.SendReplyMessage(message, string.Format(Resources.Messages.Error, type)).ConfigureAwait(false);
        }
    }
}