using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Extensions;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.VideoParser.Replies;

public record ErrorReply(Message Message, string Error) : IReply
{
    public class Handler(Bot bot) : IRequestHandler<ErrorReply, IEnumerable<Message>>
    {
        public async Task<IEnumerable<Message>> Handle(ErrorReply request, CancellationToken cancellationToken)
        {
            var (message, error) = request;
            return [await bot.SendReplyMessage(message, error).ConfigureAwait(false)];
        }
    }
}