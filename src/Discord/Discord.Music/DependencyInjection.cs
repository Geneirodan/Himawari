using System.Reflection;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Himawari.Discord.Music;

public static class DependencyInjection
{
    public static IServiceCollection AddMusicServices(
        this IServiceCollection services,
        string configSectionPath = "Discord"
    )
    {
        services.AddOptions<DiscordOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateOnStart();
        return services.AddSingleton<DiscordClient>(serviceProvider =>
        {
            var config = new DiscordConfiguration
            {
                Token = serviceProvider.GetRequiredService<IOptions<DiscordOptions>>().Value.Token,
                LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>(),
                ServiceProvider = serviceProvider
            };
            var discordClient = new DiscordClient(config);
            discordClient.UseLavalink();
            discordClient.RegisterApplicationCommandsFromAssembly(serviceProvider);
            return discordClient;
        });
    }

    private static void RegisterApplicationCommandsFromAssembly(
        this DiscordClient discordClient,
        IServiceProvider serviceProvider
    )
    {
        var config = new ApplicationCommandsConfiguration { ServiceProvider = serviceProvider };
        var appCommandExt = discordClient.UseApplicationCommands(config);
        var commands = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(ApplicationCommandsModule).IsAssignableFrom(t) && !t.IsNested);

        foreach (var command in commands)
            appCommandExt.RegisterGlobalCommands(command);
    }
}