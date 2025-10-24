using System.Reflection;
using Himawari.Alias.Services;
using Himawari.Telegram.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Alias;

public static class DependencyInjection
{
    public static IServiceCollection AddAliasGame(this IServiceCollection services)
    {
        services.AddHybridCache();
        services
            .AddCommandsFromAssemblies(Assembly.GetExecutingAssembly())
            .AddSingleton<IAliasService, AliasService>()
            .AddHttpClient<IAliasService, AliasService>();
        return services;
    }
}