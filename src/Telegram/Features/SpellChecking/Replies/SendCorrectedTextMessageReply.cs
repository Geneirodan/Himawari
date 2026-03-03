using Himawari.SpellChecking.Resources;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Html;
using Himawari.Telegram.Core.RateLimiting;
using MediatR;
using Telegram.Bot.Types;

namespace Himawari.SpellChecking.Replies;

/// <summary>Reply that sends the corrected (wrong-layout) text to the user as a reply to their message, with a "Maybe" prefix.</summary>
/// <param name="Message">The message that contained the wrong-layout text.</param>
/// <param name="Text">The corrected text.</param>
public sealed record SendCorrectedTextMessageReply(Message Message, string Text) : IReply
{
    /// <summary>Sends the corrected text as a reply with a "Maybe" prefix and <c>&lt;blockquote&gt;</c>.</summary>
    /// <remarks>User content is auto-encoded via <see cref="TelegramHtmlHandler"/> and sent with HTML parse mode per project code-style (§3).</remarks>
    public sealed class Handler(IOutgoingTelegramBot bot) : IRequestHandler<SendCorrectedTextMessageReply, IEnumerable<Message>>
    {
        /// <inheritdoc />
        public async Task<IEnumerable<Message>> Handle(SendCorrectedTextMessageReply request, CancellationToken cancellationToken)
        {
            return [await bot.SendHtmlReply(
                request.Message,
                $"{(RawHtml)Messages.Maybe}\n<blockquote>{request.Text}</blockquote>"
            ).ConfigureAwait(false)];
        }
    }
}