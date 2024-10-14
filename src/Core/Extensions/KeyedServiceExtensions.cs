using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Core.Extensions;

public static class KeyedServiceExtensions
{
    public static IServiceCollection AllowResolvingKeyedServicesAsDictionary(this IServiceCollection services) =>
        services
            .AddSingleton(services)
            .AddSingleton(typeof(KeyedServiceCache<>))
            .AddTransient(typeof(IKeyedServiceDictionary<>), typeof(KeyedServiceDictionary<>));


    public interface IKeyedServiceDictionary<TService> : IReadOnlyDictionary<string, TService>;

    private sealed class KeyedServiceDictionary<TService>(
        KeyedServiceCache<TService> keys,
        IServiceProvider provider)
        : ReadOnlyDictionary<string, TService>(Create(keys, provider)), IKeyedServiceDictionary<TService>
        where TService : notnull
    {
        private static Dictionary<string, TService> Create(KeyedServiceCache<TService> keys, IServiceProvider provider) =>
            keys.Keys.ToDictionary(x => x, provider.GetRequiredKeyedService<TService>);
    }

    private sealed class KeyedServiceCache<TService>(IServiceCollection services)
        where TService : notnull
    {
        public string[] Keys { get; } = services
            .Where(service => service.ServiceKey is string && service.ServiceType == typeof(TService))
            .Select(service => (string)service.ServiceKey!)
            .ToArray();
    }
}