using System.Reflection;
using Himawari.SpellChecking.Models;
using Himawari.SpellChecking.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Serialization;

namespace Himawari.SpellChecking;

public static class DependencyInjection
{
    public static IServiceCollection AddWrongLayoutDetection(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .Configure<SpellCheckingOptions>(configuration)
            .AddTransient<IDeserializer>(_ => new DeserializerBuilder().WithCaseInsensitivePropertyMatching().Build())
            .AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .AddSingleton<ILayoutService, LayoutService>()
            .AddSingleton<IWrongLayoutParser, WrongLayoutParser>();
    }
}