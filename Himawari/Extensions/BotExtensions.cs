using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;

namespace Himawari.Extensions;

public static class BotExtensions
{
    public static async Task<(string Command, string Text, bool ForMe)> ParseCommandAsync(this Bot bot,
        string messageText)
    {
        var commandArray =
            messageText.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var command = commandArray[0][1..].Split('@');
        var rest = commandArray.Length > 1 ? commandArray[1] : string.Empty;

        var me = await bot.GetMe();
        return (command[0], rest, command.Length == 1 || command[1] == me.Username);
    }

    public static async Task<Message> SendReplyMessage(
        this Bot bot,
        Message message,
        string text,
        ParseMode parseMode = ParseMode.None,
        IReplyMarkup? replyMarkup = null
    ) => await bot.SendTextMessage(
        chatId: message.Chat.Id,
        text: text,
        parseMode: parseMode,
        replyParameters: new ReplyParameters
        {
            MessageId = message.MessageId,
            ChatId = message.Chat.Id
        },
        replyMarkup: replyMarkup
    );
}