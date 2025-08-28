using System.Reflection;
using Himawari.SillyThings.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.SillyThings;

public static class DependencyInjection
{
    public static IServiceCollection AddSillyThings(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .Configure<SillyThingsOptions>(configuration.GetSection("SillyThings"));
}