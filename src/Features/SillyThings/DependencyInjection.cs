using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.SillyThings;

public static class DependencyInjection
{
    public static IServiceCollection AddSillyThings(this IServiceCollection services) =>
        services
            .AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
}