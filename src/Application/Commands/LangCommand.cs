using System.Globalization;
using Himawari.Application.Resources;
using Himawari.Core.Abstractions;
using Himawari.Core.Attributes;
using Himawari.Core.Extensions;
using Himawari.Core.Models;
using Himawari.Core.Options;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Application.Commands;

[BotCommand("/lang")]
public sealed record LangCommand(Message Message, string Lang) : ICommand
{


    public sealed class Handler(Bot bot, SqliteConnection connection, IOptionsMonitor<BotOptions> optionsMonitor, IMemoryCache cache) : IRequestHandler<LangCommand, Message>
    {
        private readonly string[] _supportedLanguages = optionsMonitor.CurrentValue.SupportedLocales;

        public async Task<Message> Handle(LangCommand request, CancellationToken cancellationToken)
        {
            var (message, lang) = request;
            if (!_supportedLanguages.Contains(lang))
            {
                var text = string.Format(Messages.LanguageNotFound, string.Join(", ", _supportedLanguages));
                return await bot.SendReplyMessage(message, text).ConfigureAwait(false);
            }
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            cache.Set(message.Chat.Id, new CultureInfo(lang));

            var command = new SqliteCommand(SqlCommand, connection);
            command.Parameters.AddWithValue("@Id", message.Chat.Id);
            command.Parameters.AddWithValue("@Lang", lang);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            return await bot.SendReplyMessage(message, Messages.LanguageSet).ConfigureAwait(false);
        }

        private const string SqlCommand = """
                                             INSERT INTO Chats (Id, Lang) 
                                             VALUES (@Id, @Lang)
                                             ON CONFLICT(Id) 
                                             DO 
                                               UPDATE SET Lang = @Lang
                                               WHERE Id = @Id;
                                            """;
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<LangCommand>(aliases.CurrentValue)
    {
        public override string Description => CommandDescriptions.Lang;
        public override Func<Message, string, ICommand> Factory => (message, lang) => new LangCommand(message, lang);
    }
}