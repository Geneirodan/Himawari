using System.Reflection;
using Himawari.Shared.Pipeline;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Shared;

/// <summary>
/// Dependency injection extensions for the shared MediatR pipeline (e.g. unhandled exception logging).
/// </summary>
[PublicAPI]
public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR from the calling assembly and adds <see cref="UnhandledExceptionBehavior{TRequest,TResponse}"/> to log and rethrow exceptions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddCommonPipeline(this IServiceCollection services)
        => services.AddMediatR(x =>
            x.RegisterServicesFromAssembly(Assembly.GetCallingAssembly())
                .AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>))
        );
}