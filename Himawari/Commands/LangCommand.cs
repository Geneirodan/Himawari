using Himawari.Resources;
using Himawari.Services;
using MediatR;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Commands;

public record LangCommand(Message Message, CommandInfo CommandInfo) : ICommand
{
    public class Handler(Bot bot, SqliteConnection connection) : IRequestHandler<LangCommand, Message>
    {
        public async Task<Message> Handle(LangCommand request, CancellationToken cancellationToken)
        {
            var (message, (_, _, cultureInfo)) = request;
            
            const string tableCommand = """
                                        CREATE TABLE IF NOT EXISTS Chats (
                                            Id BIGINT PRIMARY KEY, 
                                            Lang NVARCHAR(32) DEFAULT "en-US"
                                            );
                                         INSERT INTO Chats (Id, Lang) VALUES (@Id, @Lang);
                                        """;

            var createTable = new SqliteCommand(tableCommand, connection);
            createTable.Parameters.AddWithValue("@Id", message.Chat.Id);
            createTable.Parameters.AddWithValue("@Lang", cultureInfo.Name);
            await createTable.ExecuteNonQueryAsync(cancellationToken);
            return await bot.SendTextMessage(
                chatId: message.Chat.Id,
                text: Messages.ResourceManager.GetString(nameof(Messages.LanguageSet), cultureInfo) ??
                      throw new ArgumentOutOfRangeException(nameof(request)),
                replyParameters: new ReplyParameters
                {
                    MessageId = message.MessageId,
                    ChatId = message.Chat.Id
                }
            );
        }
    }
}