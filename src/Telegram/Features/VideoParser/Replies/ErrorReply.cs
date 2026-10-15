using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Extensions;
using Mediator;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.VideoParser.Replies;

public sealed record ErrorReply(Message Message, string Error) : IReply
{
    public sealed class Handler(Bot bot) : IRequestHandler<ErrorReply, IEnumerable<Message>>
    {
        public async ValueTask<IEnumerable<Message>> Handle(ErrorReply request, CancellationToken cancellationToken)
        {
            var (message, error) = request;
            return [await bot.SendReplyMessage(message, error).ConfigureAwait(false)];
        }
    }
}
