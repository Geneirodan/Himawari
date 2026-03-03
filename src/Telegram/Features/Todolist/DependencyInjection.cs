using System.Reflection;
using Himawari.Telegram.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Todolist;

/// <summary>
/// Dependency injection extensions for the Todolist feature: command options from config and Telegram command pipeline.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers <see cref="TodoCommand.Options"/> from <paramref name="configSectionPath"/> and adds the Telegram command pipeline from this assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configSectionPath">Configuration section (e.g. "Telegram:Todolist").</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTodolist(this IServiceCollection services, string configSectionPath)
    {
        services.AddOptions<TodoCommand.Options>()
            .BindConfiguration(configSectionPath)
            .ValidateOnStart();
        return services.AddTelegramCommandsFromAssemblies(Assembly.GetExecutingAssembly());
    }
}