using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;
using Testcontainers.Xunit;
using Xunit.Sdk;

namespace Himawari.CobaltTools.Tests;

[UsedImplicitly]
public class CobaltToolsContainerFixture(IMessageSink messageSink)
    : ContainerFixture<ContainerBuilder, IContainer>(messageSink)
{
    protected override ContainerBuilder Configure(ContainerBuilder builder) =>
        builder.WithImage("ghcr.io/imputnet/cobalt:latest")
            .WithPortBinding(9000, 9000)
            .WithExposedPort(9000)
            .WithEnvironment("API_URL", "http://localhost:9000");

    protected override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await Task.Delay(TimeSpan.FromSeconds(5), Xunit.TestContext.Current.CancellationToken);
    }
}