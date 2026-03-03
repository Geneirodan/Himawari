using System.Reflection;
using Himawari.Alias.Services;
using Himawari.Telegram.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Alias;

/// <summary>
/// Dependency injection extensions for the Alias game: hybrid cache, Telegram command pipeline, and <see cref="IAliasService"/>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Alias game services: hybrid cache, command pipeline from this assembly, and <see cref="IAliasService"/> (singleton + HTTP client for optional remote calls).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddAliasGame(this IServiceCollection services)
    {
        services.AddHybridCache();
        services
            .AddTelegramCommandsFromAssemblies(Assembly.GetExecutingAssembly())
            .AddSingleton<IAliasWordService, AliasWordService>()
            .AddSingleton<AliasGameHandler>()
            .AddSingleton<IAliasService, AliasService>()
            .AddSingleton<IAliasRoundTimer, AliasRoundTimerService>()
            .AddHttpClient<IAliasService, AliasService>();
        return services;
    }
}