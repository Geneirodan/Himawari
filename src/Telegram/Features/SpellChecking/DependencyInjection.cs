using System.Reflection;
using Himawari.Telegram.Core.Abstractions;
using Himawari.SpellChecking.Models;
using Himawari.SpellChecking.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Serialization;

namespace Himawari.SpellChecking;

/// <summary>
/// Dependency injection extensions for spell checking and wrong-layout detection: options, YAML deserializer, MediatR, <see cref="ILayoutService"/>, and <see cref="IWrongLayoutParser"/>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers <see cref="SpellCheckingOptions"/> from configuration, YAML deserializer, MediatR from this assembly, and layout/wrong-layout services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration root or section for spell checking.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddWrongLayoutDetection(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOptions<SpellCheckingOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services
            .AddTransient<IDeserializer>(_ => new DeserializerBuilder().WithCaseInsensitivePropertyMatching().Build())
            .AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
            .AddSingleton<ILayoutService, LayoutService>()
            .AddSingleton<IWrongLayoutParser, WrongLayoutParser>()
            .AddSingleton<ICommandLayoutCorrector, WrongLayoutCommandCorrector>();
    }
}