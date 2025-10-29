using System.Reflection;
using DisCatSharp.Lavalink;
using Himawari.Discord.Music.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Himawari.Discord.Music;

public static class DependencyInjection
{
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