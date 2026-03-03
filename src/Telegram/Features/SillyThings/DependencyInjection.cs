using System.Reflection;
using Himawari.SillyThings.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.SillyThings;

/// <summary>
/// Dependency injection extensions for the SillyThings feature (e.g. rhino GIF, SS sticker): options from config and MediatR handlers from this assembly.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers <see cref="SillyThingsOptions"/> from <paramref name="configSectionPath"/> and MediatR from this assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configSectionPath">Configuration section (e.g. "Telegram:SillyThings").</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddSillyThings(this IServiceCollection services, string configSectionPath)
    {
        services.AddOptions<SillyThingsOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateOnStart();
        services.AddSingleton<SillyThingsTriggers>(sp =>
            new SillyThingsTriggers(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SillyThingsOptions>>().Value));
        return services.AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }
}