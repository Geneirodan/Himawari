using Himawari.CobaltTools.Options;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Himawari.CobaltTools;

/// <summary>
/// Dependency injection extensions for the CobaltTools API client.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers <see cref="CobaltToolsOptions"/> (bound from <paramref name="configSectionPath"/>), <see cref="HybridCache"/>, and <see cref="ICobaltToolsService"/> as an HTTP client with standard resilience (retry, circuit breaker, timeouts).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configSectionPath">Configuration section key (e.g. "CobaltTools").</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddCobaltTools(this IServiceCollection services, string configSectionPath)
    {
        services.AddOptions<CobaltToolsOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateOnStart();

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(30),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            };
            options.MaximumPayloadBytes = 1024 * 50;
        });

        services.AddHttpClient<ICobaltToolsService, CobaltToolsService>()
            .AddStandardResilienceHandler(options =>
            {
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(300);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.MinimumThroughput = 5;
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(40);
            });

        return services;
    }
}
