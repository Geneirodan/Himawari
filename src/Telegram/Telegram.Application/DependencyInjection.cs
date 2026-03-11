using System.Reflection;
using Himawari.Telegram.Application.Commands;
using Himawari.Telegram.Application.Localization;
using Himawari.Telegram.Core;
using Himawari.Telegram.Core.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Himawari.Telegram.Application;

/// <summary>
/// Dependency injection extensions for basic Telegram commands (e.g. help, who, lang, shut up, gift, call all).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers basic command options (e.g. ShutUp from config), localization (BotMessages, <see cref="ICultureResolver"/>), the Telegram command pipeline and memory cache, and scans the executing assembly for <see cref="ICommandDescriptor"/> and command handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configSectionPath">Configuration section prefix (e.g. "Telegram" for Telegram:ShutUp).</param>
    /// <param name="configuration">Optional configuration to bind <see cref="LocalizationOptions"/> (Telegram:Localization). When provided, <see cref="ICultureResolver"/> is registered as <see cref="LanguageFallbackPolicy"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddBasicTelegramCommands(this IServiceCollection services, string configSectionPath, IConfiguration? configuration = null)
    {
        services.AddOptions<ShutUpCommand.Options>()
            .BindConfiguration($"{configSectionPath}:ShutUp")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddLocalization();
        services.AddSingleton<IBotLocalizer, BotLocalizer>();

        if (configuration is not null)
            services.Configure<Himawari.Telegram.Core.Localization.LocalizationOptions>(configuration.GetSection(Himawari.Telegram.Core.Localization.LocalizationOptions.SectionName));
        services.AddSingleton<ICultureResolver, LanguageFallbackPolicy>();

        return services.AddTelegramCommandsFromAssemblies(Assembly.GetExecutingAssembly()).AddMemoryCache();
    }
}