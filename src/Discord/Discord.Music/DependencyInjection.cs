using System.Reflection;
using DisCatSharp.Lavalink;
using Himawari.Discord.Music.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Himawari.Discord.Music;

public static class DependencyInjection
{
    public static IServiceCollection AddMusicServices(this IServiceCollection services, string configSectionPath)
    {
        services.AddOptions<LavalinkOptions>().BindConfiguration(configSectionPath).ValidateOnStart();
        return services.AddSingleton(x =>
        {
            var options = x.GetRequiredService<IOptions<LavalinkOptions>>().Value;
            return new LavalinkConfiguration
            {
                RestEndpoint = options.Endpoint,
                SocketEndpoint = options.Endpoint,
                EnableBuiltInQueueSystem = true,
            };
        });
    }
}
