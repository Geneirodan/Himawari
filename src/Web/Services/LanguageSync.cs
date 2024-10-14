using System.Globalization;
using DeepL;
using Himawari.Core.Abstractions;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Update = WTelegram.Types.Update;

namespace Himawari.Web.Services;

public interface ILanguageSync : IMessageHandler, IUpdateHandler;

public class LanguageSync(SqliteConnection connection) :  ILanguageSync
{
    private const string CommandText = """
                                       CREATE TABLE IF NOT EXISTS Chats (
                                           Id BIGINT PRIMARY KEY, 
                                           Lang NVARCHAR(32) DEFAULT "en"
                                       );
                                       SELECT Lang FROM Chats WHERE Id = @Id;
                                       """;

    private void SetLang(long chatId)
    {
        var command = new SqliteCommand(CommandText, connection);
        // command.Parameters.Add(new SqliteParameter
        // {
        //     ParameterName = "@Id",
        //     DbType = DbType.Int64,
        //     Value = chatId
        // });
        command.Parameters.AddWithValue("@Id", chatId);
        var executeScalar = command.ExecuteScalar() as string ?? LanguageCode.English;
        var culture = new CultureInfo(executeScalar);
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
    }

    public Task OnMessage(Message msg, UpdateType update)
    {
        SetLang(msg.Chat.Id);
        return Task.CompletedTask;
    }

    public Task OnUpdate(Update arg)
    {
        if (arg.CallbackQuery?.Message?.Chat.Id is {} chatId)
            SetLang(chatId);
        return Task.CompletedTask;
    }
}