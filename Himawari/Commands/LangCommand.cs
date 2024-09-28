using Himawari.Abstractions;
using Himawari.Extensions;
using Himawari.Resources;
using MediatR;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Commands;

public record LangCommand(Message Message) : ICommand
{
    public class Handler(Bot bot, SqliteConnection connection) : IRequestHandler<LangCommand, Message>
    {
        public async Task<Message> Handle(LangCommand request, CancellationToken cancellationToken)
        {
            var message= request.Message;
            
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            
            var createTable = new SqliteCommand(TableCommand, connection);
            createTable.Parameters.AddWithValue("@Id", message.Chat.Id);
            createTable.Parameters.AddWithValue("@Lang", currentCulture.Name);
            await createTable.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            return await bot.SendReplyMessage(message, Messages.LanguageSet).ConfigureAwait(false);
        }

        private const string TableCommand = """
                                            CREATE TABLE IF NOT EXISTS Chats (
                                                Id BIGINT PRIMARY KEY, 
                                                Lang NVARCHAR(32) DEFAULT "en"
                                                );
                                             INSERT INTO Chats (Id, Lang) VALUES (@Id, @Lang);
                                            """;
    }
}