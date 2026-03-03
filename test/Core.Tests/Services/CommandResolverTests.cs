using System.Globalization;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Commands;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.Options;
using Himawari.Telegram.Core.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace Himawari.Core.Tests.Services;

[TestSubject(typeof(CommandRegistry))]
public sealed class CommandResolverTests
{
    private readonly Mock<IOptionsMonitor<Aliases>> _aliasesMock = new();
    private readonly Mock<IOptions<CommandRegistryOptions>> _registryOptionsMock = new();
    private readonly ICommandResolver _resolver;
    private readonly Mock<ICommandDescriptor> _descriptorMock = new();

    public CommandResolverTests()
    {
        var aliases = new Aliases
        {
            { "/start", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "/begin", "/init" } },
            { "/help", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "/assist", "/support" } }
        };
        _aliasesMock.Setup(a => a.CurrentValue).Returns(aliases);
        _registryOptionsMock.Setup(o => o.Value).Returns(new CommandRegistryOptions { FuzzyMaxDistance = 2, ShowSuggestions = true });

        _descriptorMock.Setup(d => d.Keyword).Returns("/start");
        _descriptorMock.Setup(d => d.Description).Returns("Start the bot");
        _descriptorMock.Setup(d => d.Factory).Returns((_, _) => Mock.Of<ICommand>());
        _descriptorMock.Setup(d => d.Aliases).Returns(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "/begin", "/init" });

        _resolver = new CommandRegistry([_descriptorMock.Object], _aliasesMock.Object, _registryOptionsMock.Object);
    }

    [Fact]
    public void GetFactoryByName_ShouldReturnCorrectFactory() =>
        _resolver.GetFactoryByName("/start").ShouldNotBeNull();

    [Theory]
    [InlineData("/unknown")]
    [InlineData("help")]
    public void GetFactoryByName_ShouldReturnNull_WhenCommandDoesNotExist(string commandName) =>
        _resolver.GetFactoryByName(commandName).ShouldBeNull();

    [Theory]
    [InlineData("/begin")]
    [InlineData("/init")]
    public void GetCommandByAlias_ShouldReturnCorrectCommand(string commandName) =>
        _resolver.GetCommandByAlias(commandName).ShouldBe("/start");

    [Theory]
    [InlineData("/unknown")]
    [InlineData("help")]
    public void GetCommandByAlias_ShouldReturnNull_WhenAliasDoesNotExist(string commandName) =>
        _resolver.GetCommandByAlias(commandName).ShouldBeNull();

    [Fact]
    public void GetCommandsByCulture_ShouldReturnLocalizedCommands()
    {
        var culture = new CultureInfo("en-US");
        var commands = _resolver.GetCommandsByCulture(culture).ToArray();
        commands.ShouldNotBeEmpty();
        commands.ShouldContain(c => c.Command == "/start" && c.Description == "Start the bot");
    }

    [Fact]
    public void Resolve_ExactMatch_ReturnsExactKind()
    {
        var result = _resolver.Resolve("start".AsSpan());
        result.Kind.ShouldBe(CommandMatchKind.Exact);
        result.Entry.ShouldNotBeNull();
        result.Entry!.Keyword.ShouldBe("/start");
    }

    [Theory]
    [InlineData("begin")]
    [InlineData("init")]
    public void Resolve_AliasMatch_ReturnsAliasKind(string input)
    {
        var result = _resolver.Resolve(input.AsSpan());
        result.Kind.ShouldBe(CommandMatchKind.Alias);
        result.Entry.ShouldNotBeNull();
        result.MatchedAlias.ShouldBe(input);
    }

    [Fact]
    public void Resolve_FuzzyMatch_ReturnsFuzzyWithSuggestions()
    {
        var result = _resolver.Resolve("starrt".AsSpan());
        result.Kind.ShouldBe(CommandMatchKind.Fuzzy);
        result.Suggestions.ShouldNotBeNull();
        result.Suggestions.ShouldContain("start");
    }

    [Fact]
    public void Resolve_NotFound_ReturnsNotFoundKind()
    {
        var result = _resolver.Resolve("xyz".AsSpan());
        result.Kind.ShouldBe(CommandMatchKind.NotFound);
        result.OriginalInput.ShouldBe("xyz");
    }

    [Fact]
    public void GetAlternateLookup_ReturnsEmpty_WhenNoNaturalLanguageTriggers()
    {
        var lookup = _resolver.GetAlternateLookup();
        lookup.ShouldBeEmpty();
    }

    [Fact]
    public void GetAlternateLookup_ReturnsTriggers_WhenConfigured()
    {
        _registryOptionsMock.Setup(o => o.Value).Returns(new CommandRegistryOptions
        {
            FuzzyMaxDistance = 2,
            ShowSuggestions = true,
            NaturalLanguageTriggers = new Dictionary<string, string>
            {
                ["(?i)\\b(помоги|допоможіть)\\b"] = "help"
            }
        });
        var resolverWithNl = new CommandRegistry([_descriptorMock.Object], _aliasesMock.Object, _registryOptionsMock.Object);
        var lookup = resolverWithNl.GetAlternateLookup();
        lookup.ShouldNotBeEmpty();
        lookup["(?i)\\b(помоги|допоможіть)\\b"].ShouldBe("help");
    }
}