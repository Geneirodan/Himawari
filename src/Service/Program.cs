using DisCatSharp.Lavalink;
using Geneirodan.Observability;
using Himawari.Alias;
using Himawari.CobaltTools;
using Himawari.Discord.Core;
using Himawari.Discord.Music;
using Himawari.Discord.Music.Commands;
using Himawari.Discord.Music.Modules;
using Himawari.Service.Services;
using Himawari.Shared;
using Himawari.SillyThings;
using Himawari.Telegram.Application;
using Himawari.Telegram.Core;
using Himawari.Todolist;
using Himawari.VideoParser;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddSerilog();

var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection")
                       ?? throw new Exception("Connection string not found");
builder.Services
    .AddMediatR(x =>
    {
        x.LicenseKey = configuration["MEDIATR_KEY"];
        x.RegisterServicesFromAssemblyContaining<Program>();
    })
    .AddCommonPipeline()
    .AddSingleton<SqliteConnection>(_ => new SqliteConnection(connectionString))

    // Telegram part
    .AddTelegramBot(x => x
        .AddMessageHandler<CommandDispatcher>()
        // .AddMessageHandler<SpellCheckingDispatcher>()
        .AddMessageHandler<AliasDispatcher>()
        .AddUpdateHandler<AliasDispatcher>()
        .AddMessageHandler<VideoParsingDispatcher>()
        .AddMessageHandler<SillyThingsDispatcher>()
    )
    .AddBasicTelegramCommands("Telegram:Commands")
    .AddAliasGame()
    .AddTodolist("Telegram:Todolist")
    .AddSillyThings("Telegram:SillyThings")
    .AddCobaltTools("CobaltTools")
    .AddVideoParsing()
    // TODO: Fix critical bug with commas
    // .AddWrongLayoutDetection(configuration.GetSection("SpellChecking"))
    .AddHostedService<TelegramBackgroundService>()

    // Discord part
    .AddDiscordBot(
        configureClient: client => client.UseLavalink(),
        configSectionPath: "Discord",
        assemblies: typeof(MusicCommandsModule).Assembly)
    .AddMusicServices("Discord:Lavalink")
    .AddHostedService<DiscordBackgroundService>()

    // Observability part
    .AddSharedOpenTelemetry(configuration)
    .AddHealthChecks()
    .AddSqlite(connectionString);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await using var connection = scope.ServiceProvider.GetRequiredService<SqliteConnection>();
    await connection.OpenAsync();
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

await app.RunAsync();