using System.Reflection;
using Himawari.Shared.Pipeline;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Himawari.Shared;

[PublicAPI]
public static class DependencyInjection
{
    public static IServiceCollection AddCommonPipeline(this IServiceCollection services)
        => services.AddMediatR(x =>
            x.RegisterServicesFromAssembly(Assembly.GetCallingAssembly())
                .AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>))
        );
}