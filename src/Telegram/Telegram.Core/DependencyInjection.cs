using System.Reflection;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.Options;
using Himawari.Telegram.Core.Pipeline;
using Himawari.Telegram.Core.Services;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WTelegram;

namespace Himawari.Telegram.Core;

[PublicAPI]
public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddTelegramBot(
            Action<BotConfigurationRegistrar> configure,
            string configSectionPath = "Telegram"
        )
        {
            services
                .AddOptions<BotOptions>()
                .BindConfiguration($"{configSectionPath}:Bot")
                .ValidateOnStart()
                .ValidateDataAnnotations();

            services
                .AddOptions<Aliases>()
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
            return services;
        }

        public IServiceCollection AddTelegramCommandsFromAssemblies(params Assembly[] assemblies) =>
            services
                .AddScoped<ILanguageResolver, LanguageResolver>()
                .AddSingleton<ICommandResolver, CommandResolver>()
                .Scan(scan =>
                    scan.FromAssemblies(assemblies)
                        .AddClasses(x => x.AssignableTo<ICommandDescriptor>())
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime()
                );
    }

    public static IHost RegisterHandlers(this IHost host)
    {
        host.Services.GetRequiredService<BotConfigurationRegistrar>().RegisterHandlers(host.Services);
        return host;
    }
}
