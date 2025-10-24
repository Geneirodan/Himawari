using Geneirodan.Observability;
using Himawari.Alias;
using Himawari.CobaltTools;
using Himawari.Discord.Music;
using Himawari.Service.Services;
using Himawari.SillyThings;
using Himawari.Telegram.Application;
using Himawari.Telegram.Core;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.Options;
using Himawari.Todolist;
using Himawari.VideoParser;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddSerilog();

var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection") 
                       ?? throw new Exception("Connection string not found");
builder.Services
    .Configure<BotOptions>(configuration.GetSection("Bot"))
    .Configure<Aliases>(configuration.GetSection("Aliases"))
    .AddSingleton<SqliteConnection>(_ => new SqliteConnection(connectionString))
    .AddBasicCommands(configuration.GetSection("Commands"))
    .AddAliasGame()
    .AddTodolist(configuration)
    .AddSillyThings(configuration)
    .AddCobaltTools()
    .AddVideoParsing()
    // TODO: Fix critical bug with commas
    // .AddWrongLayoutDetection(configuration.GetSection("SpellChecking"))
    .AddTelegramBot(x => x
        .AddMessageHandler<CommandDispatcher>()
        // .AddMessageHandler<SpellCheckingDispatcher>()
        .AddMessageHandler<AliasDispatcher>()
        .AddUpdateHandler<AliasDispatcher>()
        .AddMessageHandler<VideoParsingDispatcher>()
        .AddMessageHandler<SillyThingsDispatcher>()
    )
    .AddHostedService<TelegramBackgroundService>()
    .AddHostedService<DiscordBackgroundService>()
    .AddMusicServices()
    .AddSharedOpenTelemetry(configuration)
    .AddHealthChecks()
    .AddSqlite(connectionString);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await using var connection = scope.ServiceProvider.GetRequiredService<SqliteConnection>();
    connection.Open();
    const string commandText = """
                               CREATE TABLE IF NOT EXISTS Chats (
                                   Id BIGINT PRIMARY KEY, 
                                   Lang NVARCHAR(32) DEFAULT 'en'
                               );
                               """;
    await new SqliteCommand(commandText, connection).ExecuteNonQueryAsync();
}

app.RegisterHandlers();

app.MapHealthChecks("/health");

app.Run();