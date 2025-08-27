using System.Reflection;
using Himawari.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Todolist;

public static class DependencyInjection
{
    public static IServiceCollection AddTodolist(this IServiceCollection services, IConfiguration configuration) =>
        services.AddCommandsFromAssemblies(Assembly.GetExecutingAssembly())
            .Configure<TodoCommand.Options>(configuration.GetSection("Todolist"));
}