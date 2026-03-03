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

/// <summary>
/// Dependency injection extensions for the Discord bot: options, client, and application commands from the given assemblies.
/// </summary>
[PublicAPI]
public static class DependencyInjection
{
    /// <summary>
    /// Registers <see cref="DiscordOptions"/> from config, creates and configures <see cref="DiscordClient"/> (with <paramref name="configureClient"/>), and registers application commands from <paramref name="assemblies"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureClient">Action to configure the client (e.g. UseLavalink).</param>
    /// <param name="configSectionPath">Configuration section (e.g. "Discord").</param>
    /// <param name="assemblies">Assemblies to scan for application command modules.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
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

    /// <summary>
    /// Registers global application commands from the given assemblies (modules inheriting <see cref="ApplicationCommandsModule"/>).
    /// </summary>
    /// <param name="discordClient">The Discord client.</param>
    /// <param name="serviceProvider">Service provider for command resolution.</param>
    /// <param name="assemblies">Assemblies to scan for command modules.</param>
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