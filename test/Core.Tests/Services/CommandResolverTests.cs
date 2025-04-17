using System.Globalization;
using Himawari.Core.Abstractions;
using Himawari.Core.Abstractions.Messages;
using Himawari.Core.Models;
using Himawari.Core.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace Himawari.Core.Tests.Services;

[TestSubject(typeof(CommandResolver))]
public class CommandResolverTests
{
    private readonly Mock<IOptionsMonitor<Aliases>> _aliasesMock = new();
    private readonly CommandResolver _commandResolver;
    private readonly Mock<ICommandDescriptor> _descriptorMock = new();

    public CommandResolverTests()
    {
        var aliases = new Aliases
        {
            { "/start", new HashSet<string> { "/begin", "/init" } },
            { "/help", new HashSet<string> { "/assist", "/support" } }
        };
        _aliasesMock.Setup(a => a.CurrentValue).Returns(aliases);

        _descriptorMock.Setup(d => d.Keyword).Returns("/start");
        _descriptorMock.Setup(d => d.Description).Returns("Start the bot");
        _descriptorMock.Setup(d => d.Factory).Returns((_, _) => Mock.Of<ICommand>());

        _commandResolver = new CommandResolver([_descriptorMock.Object], _aliasesMock.Object);
    }

    [Fact]
    public void GetFactoryByName_ShouldReturnCorrectFactory() => 
        _commandResolver.GetFactoryByName("/start").ShouldNotBeNull();

    [Theory]
    [InlineData("/unknown")]
    [InlineData("help")]
    public void GetFactoryByName_ShouldReturnNull_WhenCommandDoesNotExist(string commandName) => 
        _commandResolver.GetFactoryByName(commandName).ShouldBeNull();

    [Theory]
    [InlineData("/begin")]
    [InlineData("/init")]
    public void GetCommandByAlias_ShouldReturnCorrectCommand(string commandName) => 
        _commandResolver.GetCommandByAlias(commandName).ShouldBe("/start");

    [Theory]
    [InlineData("/unknown")]
    [InlineData("help")]
    public void GetCommandByAlias_ShouldReturnNull_WhenAliasDoesNotExist(string commandName) =>
        _commandResolver.GetCommandByAlias(commandName).ShouldBeNull();

    [Fact]
    public void GetCommandsByCulture_ShouldReturnLocalizedCommands()
    {
        var culture = new CultureInfo("en-US");

        var commands = _commandResolver.GetCommandsByCulture(culture).ToArray();

        commands.ShouldNotBeEmpty();
        commands.ShouldContain(c => c.Command == "/start" && c.Description == "Start the bot");
    }
}