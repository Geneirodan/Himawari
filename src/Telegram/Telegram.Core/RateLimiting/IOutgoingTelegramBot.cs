using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Himawari.Telegram.Core.RateLimiting;

/// <summary>
/// Rate-limited wrapper over the Telegram Bot API send methods. All outgoing messages pass through <see cref="TelegramRateLimiter"/> before being dispatched — prevents HTTP 429 errors.
/// </summary>
public interface IOutgoingTelegramBot
{
    /// <summary>
    /// Sends a message to the given chat. Acquires a rate-limit lease before the API call.
    /// </summary>
    /// <param name="chatId">Target chat ID.</param>
    /// <param name="text">Message text.</param>
    /// <param name="parseMode">Parse mode (default MarkdownV2).</param>
    /// <param name="replyMarkup">Optional reply markup.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the sent message.</returns>
    Task<Message> SendMessageAsync(
        long chatId,
        string text,
        ParseMode parseMode = ParseMode.MarkdownV2,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a reply to the given message. Acquires a rate-limit lease before the API call.
    /// </summary>
    /// <param name="replyTo">The message to reply to.</param>
    /// <param name="text">Reply text.</param>
    /// <param name="parseMode">Parse mode (default MarkdownV2).</param>
    /// <param name="replyMarkup">Optional reply markup.</param>
    /// <param name="replyParameters">Optional reply parameters (e.g. Quote). When <see langword="null"/>, MessageId and ChatId are taken from <paramref name="replyTo"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the sent message.</returns>
    Task<Message> SendReplyAsync(
        Message replyTo,
        string text,
        ParseMode parseMode = ParseMode.MarkdownV2,
        ReplyMarkup? replyMarkup = null,
        ReplyParameters? replyParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a sticker to the given chat (rate-limited).
    /// </summary>
    Task<Message> SendStickerAsync(
        long chatId,
        string sticker,
        ReplyParameters? replyParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an animation (GIF) to the given chat (rate-limited).
    /// </summary>
    Task<Message> SendAnimationAsync(
        long chatId,
        string animation,
        ReplyParameters? replyParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits the text of a message sent by the bot (rate-limited). Use to update the message and remove the inline keyboard after a callback.
    /// </summary>
    /// <param name="chatId">Chat that contains the message.</param>
    /// <param name="messageId">Message to edit.</param>
    /// <param name="text">New text.</param>
    /// <param name="parseMode">Parse mode (default HTML).</param>
    /// <param name="replyMarkup">New reply markup (e.g. <see langword="null"/> to remove the keyboard).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the edited message.</returns>
    Task<Message> EditMessageTextAsync(
        long chatId,
        int messageId,
        string text,
        ParseMode parseMode = ParseMode.Html,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default);
}
