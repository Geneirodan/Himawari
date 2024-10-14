using System.Globalization;
using Himawari.Core.Abstractions;
using Himawari.Core.Extensions;
using Himawari.Web.Resources;
using MediatR;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Web.Commands;

public record LangCommand(Message Message, string Lang) : ICommand
{
    public class Handler(Bot bot, SqliteConnection connection) : IRequestHandler<LangCommand, Message>
    {
        public async Task<Message> Handle(LangCommand request, CancellationToken cancellationToken)
        {
            var (message, lang) = request;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);

            var createTable = new SqliteCommand(TableCommand, connection);
            createTable.Parameters.AddWithValue("@Id", message.Chat.Id);
            createTable.Parameters.AddWithValue("@Lang", lang);
            await createTable.ExecuteNonQueryAsync(cancellationToken);
            return await bot.SendReplyMessage(message, Messages.LanguageSet);
        }

        private const string TableCommand = """
                                             INSERT INTO Chats (Id, Lang) 
                                             VALUES (@Id, @Lang)
                                             ON CONFLICT(Id) 
                                             DO 
                                               UPDATE SET Lang = @Lang
                                               WHERE Id = @Id;
                                            """;
    }
}