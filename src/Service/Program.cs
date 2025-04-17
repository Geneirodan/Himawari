using Geneirodan.Observability;
using Himawari.Alias;
using Himawari.Application;
using Himawari.Core;
using Himawari.Core.Models;
using Himawari.Core.Options;
using Himawari.Service;
using Microsoft.Data.Sqlite;

var builder = Host.CreateApplicationBuilder(args);

builder.AddSerilog();

var configuration = builder.Configuration;
builder.Services
    .Configure<BotOptions>(configuration.GetSection("Bot"))
    .Configure<Aliases>(configuration.GetSection("Aliases"))
    .AddSingleton<SqliteConnection>(_ => new SqliteConnection(configuration.GetConnectionString("DefaultConnection")))
    .AddBasicCommands(configuration.GetSection("Commands"))
    .AddAliasGame()
    // TODO: Fix critical bug with commas
    // .AddWrongLayoutDetection(configuration.GetSection("SpellChecking"))
    .AddTelegramBot(x => x
        .AddMessageHandler<CommandDispatcher>()
        // .AddMessageHandler<SpellCheckingDispatcher>()
        .AddMessageHandler<AliasDispatcher>()
        .AddUpdateHandler<AliasDispatcher>()
    )
    .AddHostedService<HostingService>()
    .AddSharedOpenTelemetry(configuration);

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

app.Run();