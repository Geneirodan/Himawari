using System.Reflection;
using Himawari;
using Himawari.Pipeline;
using Himawari.Services;
using MediatR.Pipeline;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddMediatR(x =>
    {
        x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        x.AddRequestPreProcessor(typeof(IRequestPreProcessor<>), typeof(CommandPreProcessor<>));
        x.AddRequestPostProcessor(typeof(IRequestPostProcessor<,>), typeof(MessagePostProcessor<,>));
    })
    .Configure<BotOptions>(builder.Configuration.GetSection("Bot"))
    .AddSingleton<ICommandService, CommandService>()
    .AddSingleton<IDispatcher, Dispatcher>()
    .AddSingleton(StaticServiceFactories.DbConnectionFactory)
    .AddSingleton(StaticServiceFactories.BotFactory)
    .AddHostedService<HostingService>();

var app = builder.Build();

app.MapGet("/", () => "Hello, I'm Himawari!");

await app.RunAsync();