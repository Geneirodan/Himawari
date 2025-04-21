using System.Reflection;
using Himawari.Core;
using Himawari.VideoParser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.VideoParser;

public static class DependencyInjection
{
    public static IServiceCollection AddVideoParsing(this IServiceCollection services)
    {
        services.AddCommandsFromAssemblies(Assembly.GetExecutingAssembly())
            .AddHttpClient<IVideoParser, Services.TikTokVideoParser>();
        return services;
    }
}