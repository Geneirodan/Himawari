using System.Reflection;
using Himawari.Telegram.Core;
using Himawari.VideoParser.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Himawari.VideoParser;

public static class DependencyInjection
{
    public static IServiceCollection AddVideoParsing(this IServiceCollection services)
    {
        services.AddHttpClient<IVideoParser, CobaltToolsVideoParser>().AddPolicyHandler(PolicySelector);
       return services.AddCommandsFromAssemblies(Assembly.GetExecutingAssembly());
    }

    private static IAsyncPolicy<HttpResponseMessage> PolicySelector(HttpRequestMessage _) =>
        HttpPolicyExtensions.HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}