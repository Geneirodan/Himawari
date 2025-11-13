using System.Reflection;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using Himawari.Discord.Core.Pipeline;
using JetBrains.Annotations;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Himawari.Discord.Core;

[PublicAPI]
public static class DependencyInjection
{
    public static IServiceCollection AddDiscordBot(this IServiceCollection services,
        Action<DiscordClient> configureClient, string configSectionPath = "Discord",
        params Assembly[] assemblies
    )
    {
        services.AddOptions<DiscordOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateOnStart();
        return services.AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
                .AddRequestPreProcessor(typeof(IRequestPreProcessor<>), typeof(CommandPreProcessor<>))
            )
            .AddSingleton<DiscordClient>(serviceProvider =>
            {
                var config = new DiscordConfiguration
                {
                    Token = serviceProvider.GetRequiredService<IOptions<DiscordOptions>>().Value.Token,
                    LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>(),
                    ServiceProvider = serviceProvider
                };
                var discordClient = new DiscordClient(config);
                configureClient(discordClient);
                discordClient.RegisterApplicationCommandsFromAssemblies(serviceProvider, assemblies);
                return discordClient;
            });
    }

    public static void RegisterApplicationCommandsFromAssemblies(
        this DiscordClient discordClient,
        IServiceProvider serviceProvider,
        params Assembly[] assemblies
    )
    {
        var config = new ApplicationCommandsConfiguration { ServiceProvider = serviceProvider };
        var appCommandExt = discordClient.UseApplicationCommands(config);
        var commands = assemblies.SelectMany(x => x
            .GetTypes()
            .Where(t => typeof(ApplicationCommandsModule).IsAssignableFrom(t) && !t.IsNested)
        );

        foreach (var command in commands)
            appCommandExt.RegisterGlobalCommands(command);
    }
}