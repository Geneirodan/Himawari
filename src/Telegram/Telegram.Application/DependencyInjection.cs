using System.Reflection;
using Himawari.Telegram.Application.Commands;
using Himawari.Telegram.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Telegram.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBasicCommands(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddCommandsFromAssemblies(Assembly.GetExecutingAssembly())
            .AddMemoryCache()
            .Configure<ShutUpCommand.Options>(configuration.GetSection("ShutUp"));
    }
}