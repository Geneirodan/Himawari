using System.Reflection;
using Himawari.Alias;
using Himawari.SpellChecking;
using Himawari.Web.Options;
using Himawari.Web.Pipeline;
using Himawari.Web.Services;
using Himawari.Web.Services.Abstractions;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using WTelegram;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.Services
    .Configure<BotOptions>(configuration.GetSection("Bot"))
    .AddMediatR(x =>
    {
        x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        x.AddRequestPreProcessor(typeof(IRequestPreProcessor<>), typeof(CommandPreProcessor<>));
        x.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        x.AddRequestPostProcessor(typeof(IRequestPostProcessor<,>), typeof(MessagePostProcessor<,>));
    })
    .AddSingleton<ICommandService, CommandService>()
    .AddSingleton<IDispatcher, Dispatcher>()
    .AddSingleton<ILanguageSync, LanguageSync>()
    .AddSingleton(DbConnectionFactory)
    .AddSingleton(BotFactory)
    .AddHostedService<HostingService>()
    .AddAliasGame()
    .AddSpellChecking()
    .AddHttpClient<HostingService>()
    .RemoveAllLoggers();

var app = builder.Build();

app.MapGet("/", () => "Hello, I'm Web!");

app.MapGet("/configure", async (Bot bot, ICommandService service) =>
{
    await service.ConfigureAsync(bot);
    return "I've configured my commands, master!";
});

await app.RunAsync();
return;

SqliteConnection DbConnectionFactory(IServiceProvider _) => new(configuration.GetConnectionString("DefaultConnection"));

Bot BotFactory(IServiceProvider serviceProvider)
{
    var connection = serviceProvider.GetRequiredService<SqliteConnection>();
    var botOptions = serviceProvider.GetRequiredService<IOptions<BotOptions>>().Value;
    return new Bot(botOptions.Token, botOptions.ApiId, botOptions.ApiHash, connection);
}