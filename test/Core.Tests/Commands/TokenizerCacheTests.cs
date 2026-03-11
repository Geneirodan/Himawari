using Himawari.Telegram.Core.Services;
using Shouldly;
using Xunit;

namespace Himawari.Core.Tests.Commands;

public sealed class TokenizerCacheTests
{
    [Fact]
    public void Tokenize_EmptyString_ReturnsBothEmpty()
    {
        var cache = new TokenizerCache();
        var (first, rest) = cache.Tokenize("");
        first.ShouldBeEmpty();
        rest.ShouldBeEmpty();
    }

    [Fact]
    public void Tokenize_SingleWord_ReturnsWordAndEmptyRemainder()
    {
        var cache = new TokenizerCache();
        var (first, rest) = cache.Tokenize("help");
        first.ShouldBe("help");
        rest.ShouldBeEmpty();
    }

    [Fact]
    public void Tokenize_MultipleWords_SplitsOnFirstSpace()
    {
        var cache = new TokenizerCache();
        var (first, rest) = cache.Tokenize("help me please");
        first.ShouldBe("help");
        rest.ShouldBe("me please");
    }

    [Fact]
    public void Tokenize_SameInput_ReturnsCachedResult()
    {
        var cache = new TokenizerCache();
        var r1 = cache.Tokenize("help");
        var r2 = cache.Tokenize("help");
        r1.ShouldBe(r2);
    }

    [Fact]
    public void Tokenize_WhenCapacityReached_EvictsLeastRecentlyUsed()
    {
        var cache = new TokenizerCache(3);

        cache.Tokenize("a");
        cache.Tokenize("b");
        cache.Tokenize("c");

        cache.Tokenize("a");

        cache.Tokenize("d");

        var (first, _) = cache.Tokenize("b");
        first.ShouldBe("b");
    }

    [Fact]
    public void Tokenize_CapacityOne_AlwaysEvictsPrevious()
    {
        var cache = new TokenizerCache(1);
        cache.Tokenize("first");
        cache.Tokenize("second");

        var (first, _) = cache.Tokenize("first");
        first.ShouldBe("first");
    }

    [Fact]
    public async Task Tokenize_ConcurrentAccess_NoExceptions()
    {
        var cache = new TokenizerCache(64);
        var inputs = Enumerable.Range(0, 200)
            .Select(i => $"command{i % 20} arg{i}")
            .ToArray();

        var tasks = Enumerable.Range(0, 8)
            .Select(_ => Task.Run(() =>
            {
                foreach (var input in inputs)
                    cache.Tokenize(input);
            }));

        await Should.NotThrowAsync(() => Task.WhenAll(tasks));
    }

    [Fact]
    public async Task Tokenize_ConcurrentSameKey_ReturnsSameResult()
    {
        var cache = new TokenizerCache();
        const string input = "помоги мне";

        var results = await Task.WhenAll(
            Enumerable.Range(0, 32)
                .Select(_ => Task.Run(() => cache.Tokenize(input))));

        foreach (var r in results)
        {
            r.FirstToken.ShouldBe("помоги");
            r.Remainder.ShouldBe("мне");
        }
    }
}
