using System.Reflection;
using DisCatSharp.Lavalink;
using Himawari.Discord.Music.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Himawari.Discord.Music;

/// <summary>
/// Dependency injection extensions for Discord music (Lavalink): options, MediatR behaviors, and Lavalink configuration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers <see cref="LavalinkOptions"/> from config, adds <see cref="VoiceCommandBehavior{TRequest,TResponse}"/> and <see cref="CurrentTrackCommandBehavior{TRequest,TResponse}"/> to MediatR, and registers <see cref="LavalinkConfiguration"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configSectionPath">Configuration section (e.g. "Discord:Lavalink").</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMusicServices(
        this IServiceCollection services, string configSectionPath)
    {
        services.AddOptions<LavalinkOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateOnStart();
        return services.AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
            .AddBehavior(typeof(IPipelineBehavior<,>), typeof(VoiceCommandBehavior<,>))
            .AddBehavior(typeof(IPipelineBehavior<,>), typeof(CurrentTrackCommandBehavior<,>))
        )
        .AddSingleton(x=>
        {
            var options = x.GetRequiredService<IOptions<LavalinkOptions>>().Value;
            return new LavalinkConfiguration
            {
                RestEndpoint = options.Endpoint,
                SocketEndpoint = options.Endpoint,
                EnableBuiltInQueueSystem = true
            };
        });
    }
}