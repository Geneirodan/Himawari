using Himawari.CobaltTools.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.CobaltTools;

public static class DependencyInjection
{
    public static IServiceCollection AddCobaltTools(this IServiceCollection services, string configSectionPath)
    {
        services.AddOptions<CobaltToolsOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateOnStart();
         services.AddHttpClient<ICobaltToolsService, CobaltToolsService>();
         return services;
    }
}
