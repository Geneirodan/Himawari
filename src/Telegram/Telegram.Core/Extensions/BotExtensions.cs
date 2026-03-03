using System.Runtime.CompilerServices;
using Himawari.Telegram.Core.Html;
using Himawari.Telegram.Core.RateLimiting;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static System.StringSplitOptions;

namespace Himawari.Telegram.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="Bot"/> and <see cref="IOutgoingTelegramBot"/>: parsing command text and sending reply messages.
/// </summary>
public static class BotExtensions
{
    /// <summary>
    /// Parses the message text into command keyword, remaining text, and whether the command was addressed to this bot (e.g. /cmd@BotName).
    /// </summary>
    /// <param name="bot">The bot instance.</param>
    /// <param name="messageText">Raw message text (e.g. "/start hello").</param>
    /// <param name="cachedBotUsername">Optional cached bot username; when provided, avoids a <c>GetMe</c> call for the @mention check.</param>
    /// <returns>A task that represents the asynchronous operation. The result is (Command, Text, ForMe).</returns>
    public static async Task<(string Command, string Text, bool ForMe)> ParseCommandAsync(this Bot bot,
        string messageText,
        string? cachedBotUsername = null)
    {
        var commandArray = messageText.Split(' ', 2, TrimEntries | RemoveEmptyEntries);

        var command = commandArray[0].Split('@');
        var rest = commandArray.Length > 1 ? commandArray[1] : string.Empty;

        string? username = cachedBotUsername;
        if (username is null && command.Length > 1)
        {
            var me = await bot.GetMe().ConfigureAwait(false);
            username = me?.Username;
        }
        return (command[0], rest, command.Length == 1 || (username is not null && string.Equals(command[1], username, StringComparison.Ordinal)));
    }

    /// <summary>
    /// Sends a reply to the given message in the same chat with optional parse mode and reply markup.
    /// </summary>
    /// <param name="bot">The bot instance.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="text">Reply text.</param>
    /// <param name="parseMode">Parse mode (default MarkdownV2). For user-supplied content use <see cref="SendHtmlReply"/> or <see cref="SendHtmlMessage"/> with HTML encoding.</param>
    /// <param name="replyMarkup">Optional keyboard or inline markup.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the sent message.</returns>
    public static async Task<Message> SendReplyMessage(
        this Bot bot,
        Message message,
        string text,
        ParseMode parseMode = ParseMode.MarkdownV2,
        ReplyMarkup? replyMarkup = null
    ) => await bot.SendMessage(
        chatId: message.Chat.Id,
        text: text,
        parseMode: parseMode,
        replyParameters: new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat.Id },
        replyMarkup: replyMarkup
    ).ConfigureAwait(false);

    /// <summary>
    /// Sends a reply using HTML parse mode with automatic encoding of all interpolated values via <see cref="TelegramHtmlHandler"/>.
    /// </summary>
    /// <param name="bot">The bot instance.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="handler">An interpolated string where values are auto-encoded and literal HTML tags are preserved; use <see cref="RawHtml"/> for trusted content.</param>
    /// <param name="replyMarkup">Optional keyboard or inline markup.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the sent message.</returns>
    public static Task<Message> SendHtmlReply(
        this Bot bot,
        Message message,
        [InterpolatedStringHandlerArgument] TelegramHtmlHandler handler,
        ReplyMarkup? replyMarkup = null)
    {
        var text = handler.Build();
        return SendHtmlReplyCore(bot, message, text, replyMarkup);
    }

    private static async Task<Message> SendHtmlReplyCore(
        Bot bot,
        Message message,
        string text,
        ReplyMarkup? replyMarkup)
        => await bot.SendMessage(
            chatId: message.Chat.Id,
            text: text,
            parseMode: ParseMode.Html,
            replyParameters: new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat.Id },
            replyMarkup: replyMarkup
        ).ConfigureAwait(false);

    /// <summary>
    /// Sends a message to the given chat using HTML parse mode with automatic encoding of all interpolated values via <see cref="TelegramHtmlHandler"/>.
    /// </summary>
    /// <param name="bot">The bot instance.</param>
    /// <param name="chatId">The target chat ID.</param>
    /// <param name="handler">An interpolated string where values are auto-encoded and literal HTML tags are preserved; use <see cref="RawHtml"/> for trusted content.</param>
    /// <param name="replyMarkup">Optional keyboard or inline markup.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the sent message.</returns>
    public static Task<Message> SendHtmlMessage(
        this Bot bot,
        long chatId,
        [InterpolatedStringHandlerArgument] TelegramHtmlHandler handler,
        ReplyMarkup? replyMarkup = null)
    {
        var text = handler.Build();
        return SendHtmlMessageCore(bot, chatId, text, replyMarkup);
    }

    private static async Task<Message> SendHtmlMessageCore(
        Bot bot,
        long chatId,
        string text,
        ReplyMarkup? replyMarkup)
        => await bot.SendMessage(chatId, text, ParseMode.Html, replyMarkup: replyMarkup).ConfigureAwait(false);

    // --- IOutgoingTelegramBot (rate-limited) extensions ---

    /// <summary>
    /// Sends a reply to the given message (rate-limited). For user-supplied content use <see cref="SendHtmlReply(IOutgoingTelegramBot,Message,TelegramHtmlHandler,ReplyMarkup?)"/>.
    /// </summary>
    /// <param name="replyParameters">Optional (e.g. Quote). When <see langword="null"/>, MessageId and ChatId are taken from <paramref name="message"/>.</param>
    public static Task<Message> SendReplyMessage(
        this IOutgoingTelegramBot sender,
        Message message,
        string text,
        ParseMode parseMode = ParseMode.MarkdownV2,
        ReplyMarkup? replyMarkup = null,
        ReplyParameters? replyParameters = null,
        CancellationToken cancellationToken = default)
        => sender.SendReplyAsync(message, text, parseMode, replyMarkup, replyParameters, cancellationToken);

    /// <summary>
    /// Sends a reply using HTML parse mode with automatic encoding via <see cref="TelegramHtmlHandler"/> (rate-limited).
    /// </summary>
    public static Task<Message> SendHtmlReply(
        this IOutgoingTelegramBot sender,
        Message message,
        [InterpolatedStringHandlerArgument] TelegramHtmlHandler handler,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
        => sender.SendReplyAsync(message, handler.Build(), ParseMode.Html, replyMarkup, null, cancellationToken);

    /// <summary>
    /// Sends a message to the given chat using HTML parse mode with automatic encoding via <see cref="TelegramHtmlHandler"/> (rate-limited).
    /// </summary>
    public static Task<Message> SendHtmlMessage(
        this IOutgoingTelegramBot sender,
        long chatId,
        [InterpolatedStringHandlerArgument] TelegramHtmlHandler handler,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
        => sender.SendMessageAsync(chatId, handler.Build(), ParseMode.Html, replyMarkup, cancellationToken);
}