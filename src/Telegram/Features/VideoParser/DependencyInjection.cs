using System.Reflection;
using Himawari.Telegram.Core;
using Himawari.VideoParser.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Himawari.VideoParser;

/// <summary>
/// Dependency injection extensions for the video parsing feature: <see cref="IVideoParser"/> (CobaltTools) with retry policy and Telegram command pipeline.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers <see cref="IVideoParser"/> as <see cref="CobaltToolsVideoParser"/> with HTTP client and retry policy, and adds the Telegram command pipeline from this assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddVideoParsing(this IServiceCollection services)
    {
        services.AddHttpClient<IVideoParser, CobaltToolsVideoParser>().AddPolicyHandler(PolicySelector);
        return services.AddTelegramCommandsFromAssemblies(Assembly.GetExecutingAssembly());
    }

    private static IAsyncPolicy<HttpResponseMessage> PolicySelector(HttpRequestMessage _) =>
        HttpPolicyExtensions.HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}