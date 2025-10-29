using System.Reflection;
using Himawari.Telegram.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Todolist;

public static class DependencyInjection
{
    public static IServiceCollection AddTodolist(this IServiceCollection services, string configSectionPath)
    {
        services.AddOptions<TodoCommand.Options>()
            .BindConfiguration(configSectionPath)
            .ValidateOnStart();
        return services.AddTelegramCommandsFromAssemblies(Assembly.GetExecutingAssembly());
    }
}