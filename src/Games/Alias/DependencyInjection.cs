using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Alias;

public static class DependencyInjection
{
    public static IServiceCollection AddAliasGame(this IServiceCollection services) =>
        services.AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .AddSingleton<IAliasService, AliasService>();
}