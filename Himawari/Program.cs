using System.Reflection;
using Himawari.Abstractions.Services;
using Himawari.Options;
using Himawari.Pipeline;
using Himawari.Services;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using WTelegram;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.Services
    .Configure<BotOptions>(configuration.GetSection("Bot"))
    .Configure<ApiOptions>(configuration.GetSection("Apis")).AddMediatR(x =>
    {
        x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        x.AddRequestPreProcessor(typeof(IRequestPreProcessor<>), typeof(CommandPreProcessor<>));
        x.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        x.AddRequestPostProcessor(typeof(IRequestPostProcessor<,>), typeof(MessagePostProcessor<,>));
    })
    .AddSingleton<ICommandService, CommandService>()
    .AddSingleton<IDispatcher, Dispatcher>()
    .AddSingleton<IAliasService, AliasService>()
    .AddSingleton(DbConnectionFactory)
    .AddSingleton(BotFactory)
    .AddHostedService<HostingService>()
    .AddHttpClient<HostingService>()
    .RemoveAllLoggers();

var app = builder.Build();

app.MapGet("/", () => "Hello, I'm Himawari!");

app.MapGet("/configure", (Bot bot, ICommandService service) =>
{
    service.ConfigureAsync(bot);
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