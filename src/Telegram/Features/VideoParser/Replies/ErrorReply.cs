using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.RateLimiting;
using MediatR;
using Telegram.Bot.Types;

namespace Himawari.VideoParser.Replies;

/// <summary>Reply that sends the error text as a reply to the original message (e.g. when video parsing failed).</summary>
/// <param name="Message">The message that triggered the parse.</param>
/// <param name="Error">Error message to show the user.</param>
public sealed record ErrorReply(Message Message, string Error) : IReply
{
    /// <inheritdoc />
    public sealed class Handler(IOutgoingTelegramBot bot) : IRequestHandler<ErrorReply, IEnumerable<Message>>
    {
        public async Task<IEnumerable<Message>> Handle(ErrorReply request, CancellationToken cancellationToken)
        {
            var (message, error) = request;
            return [await bot.SendReplyMessage(message, error).ConfigureAwait(false)];
        }
    }
}