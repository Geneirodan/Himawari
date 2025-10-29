using System.Reflection;
using Himawari.SillyThings.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.SillyThings;

public static class DependencyInjection
{
    public static IServiceCollection AddSillyThings(this IServiceCollection services, string configSectionPath)
    {
        services.AddOptions<SillyThingsOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateOnStart();
        return services.AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }
}