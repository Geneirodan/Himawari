using System.Reflection;
using Himawari.Core;
using Himawari.VideoParser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.VideoParser;

public static class DependencyInjection
{
    public static IServiceCollection AddVideoParsing(this IServiceCollection services)
    {
        services.AddHttpClient<IVideoParser, TikTokVideoParser>();
        services.AddHttpClient<IVideoParser, YouTubeVideoParser>();
        return services.AddCommandsFromAssemblies(Assembly.GetExecutingAssembly());
    }
}