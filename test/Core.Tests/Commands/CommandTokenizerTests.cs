using Himawari.Telegram.Core.Commands;
using Shouldly;
using Xunit;

namespace Himawari.Core.Tests.Commands;

public sealed class CommandTokenizerTests
{
    [Theory]
    [InlineData(null, "", "")]
    [InlineData("", "", "")]
    [InlineData("   ", "", "")]
    public void Tokenize_EmptyOrWhitespace_ReturnsEmptyTuple(string? input, string expectedFirst, string expectedRemainder)
    {
        var (first, remainder) = CommandTokenizer.Tokenize(input);
        first.ShouldBe(expectedFirst);
        remainder.ShouldBe(expectedRemainder);
    }

    [Fact]
    public void Tokenize_SingleWord_ReturnsWordAndEmptyRemainder()
    {
        var (first, remainder) = CommandTokenizer.Tokenize("help");
        first.ShouldBe("help");
        remainder.ShouldBe("");
    }

    [Fact]
    public void Tokenize_TwoWords_ReturnsFirstAndRemainder()
    {
        var (first, remainder) = CommandTokenizer.Tokenize("help me");
        first.ShouldBe("help");
        remainder.ShouldBe("me");
    }

    [Fact]
    public void Tokenize_WithLeadingTrailingSpaces_TrimsCorrectly()
    {
        var (first, remainder) = CommandTokenizer.Tokenize("  помоги   пожалуйста  ");
        first.ShouldBe("помоги");
        remainder.ShouldBe("пожалуйста");
    }

    [Fact]
    public void Tokenize_PhraseWithMultipleSpaces_SplitsOnFirstRun()
    {
        var (first, remainder) = CommandTokenizer.Tokenize("help   me   please");
        first.ShouldBe("help");
        remainder.ShouldBe("me   please");
    }
}
