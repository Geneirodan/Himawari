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
/// Dependency injection extensions for the Telegram bot: registers options, WTelegram <see cref="Bot"/>, command pipeline, and channel processing.
/// </summary>
[PublicAPI]
public static class DependencyInjection
{
    /// <summary>
    /// Registers Telegram bot options, aliases, and the <see cref="Bot"/> instance; delegates handler wiring to <paramref name="configure"/>.
    /// </summary>
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
    /// Registers MediatR pipeline for Telegram commands and scans <paramref name="assemblies"/> for <see cref="ICommandDescriptor"/>; wires /lang persistence and command resolver.
    /// </summary>
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
    /// Registers the bounded-channel update pipeline and per-chat concurrency control for Telegram updates. Bind options from <c>Telegram:Pipeline</c>; call after <see cref="AddTelegramBot"/>.
    /// </summary>
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
    /// Subscribes all registered Telegram message and update handlers to the bot at host startup.
    /// </summary>
    public static IHost RegisterHandlers(this IHost host)
    {
        host.Services.GetRequiredService<BotConfigurationRegistrar>().RegisterHandlers(host.Services);
        return host;
    }
}