using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static System.StringSplitOptions;

namespace Himawari.Telegram.Core.Extensions;

public static class BotExtensions
{
    public static async Task<(string Command, string Text, bool ForMe)> ParseCommandAsync(this Bot bot,
        string messageText)
    {
        var commandArray = messageText.Split(' ', 2, TrimEntries | RemoveEmptyEntries);

        var command = commandArray[0].Split('@');
        var rest = commandArray.Length > 1 ? commandArray[1] : string.Empty;

        var me = await bot.GetMe().ConfigureAwait(false);
        return (command[0], rest, command.Length == 1 || string.Equals(command[1], me.Username, StringComparison.Ordinal));
    }

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
}