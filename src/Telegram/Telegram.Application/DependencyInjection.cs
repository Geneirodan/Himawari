using System.Reflection;
using Himawari.Telegram.Application.Commands;
using Himawari.Telegram.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Telegram.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBasicTelegramCommands(this IServiceCollection services, string configSectionPath)
    {
        services.AddOptions<ShutUpCommand.Options>()
            .BindConfiguration($"{configSectionPath}:ShutUp")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services.AddTelegramCommandsFromAssemblies(Assembly.GetExecutingAssembly()).AddMemoryCache();
    }
}