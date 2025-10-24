using Himawari.CobaltTools.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.CobaltTools;

public static class DependencyInjection
{
    public static IServiceCollection AddCobaltTools(this IServiceCollection services)
    {
        services.AddOptions<CobaltToolsOptions>()
            .BindConfiguration("VideoParsing")
            .ValidateOnStart();
         services.AddHttpClient<ICobaltToolsService, CobaltToolsService>();
         return services;
    }
}
