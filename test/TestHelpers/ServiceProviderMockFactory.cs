using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Himawari.TestHelpers;

public static class ServiceProviderMockFactory
{
    public static Mock<IServiceProvider> CreateServiceProvider()
    {
        var serviceProvider = new Mock<IServiceProvider>();
        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(serviceScope.Object);

        serviceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);
        return serviceProvider;
    }
}