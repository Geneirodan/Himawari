﻿using System.Reflection;
using Himawari.Core.Abstractions;
using Himawari.Core.Options;
using Himawari.Core.Pipeline;
using Himawari.Core.Services;
using JetBrains.Annotations;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WTelegram;

namespace Himawari.Core;

[PublicAPI]
public static class DependencyInjection
{
    public static IServiceCollection AddTelegramBot(
        this IServiceCollection services,
        Action<BotConfigurationRegistrar> configure
    )
    {
        var configuration = new BotConfigurationRegistrar();
        configure(configuration);
        configuration.RegisterHandlers(services);
        services.AddSingleton(serviceProvider =>
        {
            var connection = serviceProvider.GetRequiredService<SqliteConnection>();
            var botOptions = serviceProvider.GetRequiredService<IOptions<BotOptions>>().Value;
            return new Bot(botOptions.Token, botOptions.ApiId, botOptions.ApiHash, connection);
        });
        return services;
    }

    public static IServiceCollection AddCommandsFromAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies
    ) => services
        .AddMediatR(x => x.RegisterServicesFromAssemblies(assemblies)
            .AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>))
            .AddRequestPreProcessor(typeof(IRequestPreProcessor<>), typeof(CommandPreProcessor<>))
            .AddBehavior(typeof(IPipelineBehavior<,>), typeof(LocalizationBehavior<,>))
            .AddRequestPostProcessor(typeof(IRequestPostProcessor<,>), typeof(MessagePostProcessor<,>))
        )
        .AddScoped<ILanguageResolver, LanguageResolver>()
        .AddSingleton<ICommandResolver, CommandResolver>()
        .Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(x => x.AssignableTo<ICommandDescriptor>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
        );

    public static IHost RegisterHandlers(this IHost host)
    {
        host.Services.GetRequiredService<BotConfigurationRegistrar>().RegisterHandlers(host.Services);
        return host;
    }
}