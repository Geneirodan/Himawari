using System.Reflection;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Commands;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.Options;
using Himawari.Telegram.Core.Localization;
using Himawari.Telegram.Core.Pipeline;
using Himawari.Telegram.Core.RateLimiting;
using Himawari.Telegram.Core.Services;
using JetBrains.Annotations;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WTelegram;

namespace Himawari.Telegram.Core;

/// <summary>
/// Dependency injection extensions for the Telegram bot: bot options, handlers, WTelegram <see cref="Bot"/>, and optional command pipeline.
/// </summary>
[PublicAPI]
public static class DependencyInjection
{
    /// <summary>
    /// Registers Telegram bot options, aliases, and the <see cref="Bot"/> instance; configures message and update handlers via <paramref name="configure"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to add message/update handlers to the registrar.</param>
    /// <param name="configSectionPath">Configuration section prefix (e.g. "Telegram" binds Telegram:Bot, Telegram:Aliases).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTelegramBot(
        this IServiceCollection services,
        Action<BotConfigurationRegistrar> configure,
        string configSectionPath = "Telegram"
    )
    {
        services.AddOptions<BotOptions>()
            .BindConfiguration($"{configSectionPath}:Bot")
            .ValidateOnStart()
            .ValidateDataAnnotations();

        services.AddOptions<Aliases>()
            .BindConfiguration($"{configSectionPath}:Aliases")
            .ValidateOnStart()
            .ValidateDataAnnotations();

        var configuration = new BotConfigurationRegistrar();
        configure(configuration);
        configuration.RegisterHandlers(services);
        services.AddSingleton(serviceProvider =>
        {
            var connection = serviceProvider.GetRequiredService<SqliteConnection>();
            var botOptions = serviceProvider.GetRequiredService<IOptions<BotOptions>>().Value;
            return new Bot(botOptions.Token, botOptions.ApiId, botOptions.ApiHash, connection);
        });
        services.AddSingleton<IBotIdentity, BotIdentity>();
        return services;
    }

    /// <summary>
    /// Registers MediatR command pipeline (pre-processor, localization, post-processor), <see cref="ILanguageRepository"/> and <see cref="IExplicitLanguageResolver"/> for /lang persistence, <see cref="ICommandResolver"/>, and scans assemblies for <see cref="ICommandDescriptor"/> implementations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for commands and descriptors.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTelegramCommandsFromAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        services.AddOptions<CommandRegistryOptions>()
            .BindConfiguration("Telegram:Commands")
            .ValidateDataAnnotations();
        return services
            .AddMediatR(x => x.RegisterServicesFromAssemblies(assemblies)
            .AddRequestPreProcessor(typeof(IRequestPreProcessor<>), typeof(CommandPreProcessor<>))
            .AddBehavior(typeof(IPipelineBehavior<,>), typeof(CultureBehavior<,>))
            .AddBehavior(typeof(IPipelineBehavior<,>), typeof(LocalizationBehavior<,>))
            .AddRequestPostProcessor(typeof(IRequestPostProcessor<,>), typeof(MessagePostProcessor<,>))
        )
        .AddScoped<ILanguageRepository, ChatLanguageRepository>()
        .AddScoped<IExplicitLanguageResolver, ExplicitLanguageResolver>()
        .AddSingleton<ITokenizerCache, TokenizerCache>()
        .Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(x => x.AssignableTo<ICommandDescriptor>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
        )
        .AddSingleton<ICommandResolver, CommandRegistry>();
    }

    /// <summary>
    /// Registers the four-layer channel pipeline: bounded channel, worker pool, per-chat semaphore (for MediatR), and message handler provider. Bind options from <c>Telegram:Pipeline</c>. Call after <see cref="AddTelegramBot"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTelegramChannelPipeline(this IServiceCollection services)
    {
        services.AddOptions<ChannelPipelineOptions>()
            .BindConfiguration("Telegram:Pipeline")
            .ValidateDataAnnotations();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<TelegramUpdateChannel>();
        services.AddSingleton<KeyedSemaphore<long>>();
        services.AddHostedService<TelegramUpdateWorkerService>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ChatConcurrencyBehavior<,>));
        services.AddSingleton<TelegramRateLimiter>();
        services.AddSingleton<RateLimitedTelegramBot>();
        services.AddSingleton<IOutgoingTelegramBot>(sp => sp.GetRequiredService<RateLimitedTelegramBot>());
        return services;
    }

    /// <summary>
    /// Subscribes all registered Telegram message and update handlers to the bot. Call after building the host (e.g. <c>app.RegisterHandlers()</c>).
    /// </summary>
    /// <param name="host">The host whose services contain <see cref="BotConfigurationRegistrar"/> and <see cref="Bot"/>.</param>
    /// <returns>The same <see cref="IHost"/> for chaining.</returns>
    public static IHost RegisterHandlers(this IHost host)
    {
        host.Services.GetRequiredService<BotConfigurationRegistrar>().RegisterHandlers(host.Services);
        return host;
    }
}