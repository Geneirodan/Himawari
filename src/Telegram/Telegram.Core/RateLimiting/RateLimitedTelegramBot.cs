using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;

namespace Himawari.Telegram.Core.RateLimiting;

/// <summary>
/// Implementation of <see cref="IOutgoingTelegramBot"/> that acquires a rate-limit token before every Telegram API call and releases it after the response is received.
/// </summary>
internal sealed class RateLimitedTelegramBot(
    Bot bot,
    TelegramRateLimiter limiter,
    ILogger<RateLimitedTelegramBot> logger) : IOutgoingTelegramBot
{
    /// <inheritdoc />
    public async Task<Message> SendMessageAsync(
        long chatId,
        string text,
        ParseMode parseMode = ParseMode.MarkdownV2,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        using var lease = await limiter.AcquireAsync(chatId, cancellationToken).ConfigureAwait(false);
        try
        {
            return await bot.SendMessage(chatId, text, parseMode, replyMarkup: replyMarkup).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex.Message.Contains("429", StringComparison.Ordinal))
        {
            logger.LogWarning(ex, "429 from Telegram for chat {ChatId}", chatId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Message> SendReplyAsync(
        Message replyTo,
        string text,
        ParseMode parseMode = ParseMode.MarkdownV2,
        ReplyMarkup? replyMarkup = null,
        ReplyParameters? replyParameters = null,
        CancellationToken cancellationToken = default)
    {
        using var lease = await limiter.AcquireAsync(replyTo.Chat.Id, cancellationToken).ConfigureAwait(false);
        try
        {
            var rp = replyParameters ?? new ReplyParameters { MessageId = replyTo.MessageId, ChatId = replyTo.Chat.Id };
            return await bot.SendMessage(
                chatId: replyTo.Chat.Id,
                text: text,
                parseMode: parseMode,
                replyParameters: rp,
                replyMarkup: replyMarkup
            ).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex.Message.Contains("429", StringComparison.Ordinal))
        {
            logger.LogWarning(ex, "429 from Telegram for chat {ChatId}", replyTo.Chat.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Message> SendStickerAsync(
        long chatId,
        string sticker,
        ReplyParameters? replyParameters = null,
        CancellationToken cancellationToken = default)
    {
        using var lease = await limiter.AcquireAsync(chatId, cancellationToken).ConfigureAwait(false);
        return await bot.SendSticker(chatId, sticker, replyParameters: replyParameters).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Message> SendAnimationAsync(
        long chatId,
        string animation,
        ReplyParameters? replyParameters = null,
        CancellationToken cancellationToken = default)
    {
        using var lease = await limiter.AcquireAsync(chatId, cancellationToken).ConfigureAwait(false);
        return await bot.SendAnimation(chatId, animation, replyParameters: replyParameters).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Message> EditMessageTextAsync(
        long chatId,
        int messageId,
        string text,
        ParseMode parseMode = ParseMode.Html,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        using var lease = await limiter.AcquireAsync(chatId, cancellationToken).ConfigureAwait(false);
        return await bot.EditMessageText(chatId, messageId, text, parseMode, entities: null, replyMarkup: (InlineKeyboardMarkup?)replyMarkup).ConfigureAwait(false);
    }
}
