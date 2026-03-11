using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Himawari.Alias.Enums;
using Himawari.Alias.Options;
using Microsoft.Extensions.Options;

namespace Himawari.Alias.Services;

/// <summary>
/// Default implementation of <see cref="IAliasService"/>: fetches words from config categories or an external generator, caches them per chat, and verifies guesses. When <see cref="AliasGameHandler"/> and <see cref="IAliasWordService"/> are configured (AliasGame:Categories), uses them for those categories (60s round, Fisher–Yates); otherwise uses <see cref="AliasWordSetsOptions"/> and teoset.com. Games older than <see cref="MaxGameDuration"/> are considered dead and cleaned up.
/// </summary>
/// <param name="client">HTTP client used for remote word source (e.g. teoset.com) when category is "random" or missing.</param>
/// <param name="wordSetsOptions">Configured word sets per category.</param>
/// <param name="wordService">Optional word service for AliasGame:Categories; when present and category exists, <paramref name="gameHandler"/> is used.</param>
/// <param name="gameHandler">Optional game handler for per-chat sessions and rounds; used when <paramref name="wordService"/> has the selected category.</param>
public sealed partial class AliasService(
    HttpClient client,
    IOptions<AliasWordSetsOptions> wordSetsOptions,
    IAliasWordService? wordService,
    AliasGameHandler? gameHandler) : IAliasService
{
    private static readonly TimeSpan MaxGameDuration = TimeSpan.FromHours(2);
    private static readonly ConcurrentDictionary<long, long?> PresenterIds = new();
    private static readonly ConcurrentDictionary<long, string> Words = new();
    private static readonly ConcurrentDictionary<long, int> CorrectCounts = new();
    private static readonly ConcurrentDictionary<long, DateTime> StartedAtUtc = new();
    private static readonly ConcurrentDictionary<long, string> PendingCategory = new();
    private static readonly ConcurrentDictionary<long, string?> GameCategory = new();
    private static readonly ConcurrentDictionary<long, string?> PendingNextWordFromHandler = new();

    private IDictionary<string, List<string>> WordSets => wordSetsOptions.Value.WordSets;

    private bool UseNewHandler(long chatId) =>
        gameHandler is not null && wordService is not null && gameHandler.HasSession(chatId);

    public void SetCategory(long chatId, string categoryKey)
    {
        PendingCategory[chatId] = categoryKey;
    }

    public string? GetCategory(long chatId) => PendingCategory.GetValueOrDefault(chatId);

    public async Task<string> StartAsync(long chatId, long presenterId, string? categoryKey = null, CancellationToken cancellationToken = default)
    {
        var category = categoryKey ?? PendingCategory.GetValueOrDefault(chatId);
        PendingCategory.TryRemove(chatId, out _);
        PresenterIds[chatId] = presenterId;
        StartedAtUtc[chatId] = DateTime.UtcNow;
        GameCategory[chatId] = category;

        if (wordService is not null && gameHandler is not null && !string.IsNullOrEmpty(category) && wordService.HasCategory(category))
        {
            CorrectCounts[chatId] = 0;
            var culture = CultureInfo.CurrentUICulture.Name;
            var first = gameHandler.StartRound(chatId, category, culture);
            return first ?? string.Empty;
        }

        CorrectCounts[chatId] = 0;
        return (await NextWordAsync(chatId, cancellationToken).ConfigureAwait(false)) ?? string.Empty;
    }

    public void EndGame(long chatId)
    {
        gameHandler?.EndRound(chatId);
        PendingNextWordFromHandler.TryRemove(chatId, out _);
        Cleanup(chatId);
    }

    public int GetCorrectCount(long chatId) =>
        UseNewHandler(chatId) ? gameHandler!.GetScore(chatId) : CorrectCounts.GetValueOrDefault(chatId);

    public int IncrementCorrectCount(long chatId)
    {
        if (UseNewHandler(chatId))
        {
            var (nextWord, score) = gameHandler!.WordGuessed(chatId);
            if (nextWord is not null)
                PendingNextWordFromHandler[chatId] = nextWord;
            return score;
        }
        var next = CorrectCounts.GetValueOrDefault(chatId) + 1;
        CorrectCounts[chatId] = next;
        return next;
    }

    public long? GetPresenterId(long chatId)
    {
        if (!PresenterIds.TryGetValue(chatId, out var presenterId))
            return null;
        if (StartedAtUtc.TryGetValue(chatId, out var started) && DateTime.UtcNow - started >= MaxGameDuration)
        {
            Cleanup(chatId);
            return null;
        }
        return presenterId;
    }

    private void Cleanup(long chatId)
    {
        PendingNextWordFromHandler.TryRemove(chatId, out _);
        Words.TryRemove(chatId, out _);
        PresenterIds.TryRemove(chatId, out _);
        CorrectCounts.TryRemove(chatId, out _);
        StartedAtUtc.TryRemove(chatId, out _);
        GameCategory.TryRemove(chatId, out _);
        PendingCategory.TryRemove(chatId, out _);
    }

    public async Task<string?> NextWordAsync(long chatId, CancellationToken cancellationToken = default)
    {
        if (UseNewHandler(chatId))
        {
            if (PendingNextWordFromHandler.TryRemove(chatId, out var pending) && pending is not null)
                return pending;
            return gameHandler!.WordSkipped(chatId);
        }

        var category = GameCategory.GetValueOrDefault(chatId);
        if (!string.IsNullOrEmpty(category) && !string.Equals(category, "random", StringComparison.OrdinalIgnoreCase)
            && WordSets.TryGetValue(category, out var list) && list.Count > 0)
        {
            var picked = list[Random.Shared.Next(list.Count)];
            Words[chatId] = picked;
            return picked;
        }

        return await FetchFromTeosetAsync(chatId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string?> FetchFromTeosetAsync(long chatId, CancellationToken cancellationToken)
    {
        var nameValueCollection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["qu_words"] = "1",
            ["type_words"] = "objects",
            ["order"] = "in_order",
            ["done"] = "Create"
        };
        var formContent = new FormUrlEncodedContent(nameValueCollection);
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var requestUri = $"https://teoset.com/word-generator/lang.{culture}";
        using var response = await client.PostAsync(requestUri, formContent, cancellationToken).ConfigureAwait(false);
        var str = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var word = ResultRegex.Matches(str).FirstOrDefault()?.Groups[1].Value.Trim();
        if (word is not null)
            Words[chatId] = word;
        return word;
    }

    public async Task<string?> GetOrCreateCurrentWordAsync(long chatId, CancellationToken cancellationToken = default)
    {
        if (UseNewHandler(chatId))
            return gameHandler!.GetCurrentWord(chatId);
        return Words.GetValueOrDefault(chatId) ?? await NextWordAsync(chatId, cancellationToken).ConfigureAwait(false);
    }

    public Guess VerifyWord(long chatId, string word)
    {
        var current = UseNewHandler(chatId) ? gameHandler!.GetCurrentWord(chatId) : Words.GetValueOrDefault(chatId);
        if (current is null)
            return Guess.Incorrect;

        var errors = (current.Length - word.Length) switch
        {
            0 => TestEquality(word, current),
            1 => TestPartialEquality(word, current),
            -1 => TestPartialEquality(current, word),
            _ => 2
        };

        return errors switch
        {
            0 => Guess.Correct,
            1 => Guess.Partial,
            _ => Guess.Incorrect
        };
    }

    private static int TestEquality(string word, string current)
    {
        var errors = 0;
        for (var i = 0; i < current.Length; i++)
        {
            var a = current[i];
            var b = word[i];
            var aSpan = MemoryMarshal.CreateReadOnlySpan(ref a, 1);
            var bSpan = MemoryMarshal.CreateReadOnlySpan(ref b, 1);
            if (aSpan.CompareTo(bSpan, StringComparison.InvariantCultureIgnoreCase) == 0)
                continue;
            if (++errors > 1)
                return errors;
        }

        return errors;
    }

    private static int TestPartialEquality(string shorterWord, string longerWord)
    {
        var errors = 0;
        for (var i = 0; i < shorterWord.Length; i++)
        {
            var a = longerWord[i + errors];
            var b = shorterWord[i];
            var aSpan = MemoryMarshal.CreateReadOnlySpan(ref a, 1);
            var bSpan = MemoryMarshal.CreateReadOnlySpan(ref b, 1);
            if (aSpan.CompareTo(bSpan, StringComparison.InvariantCultureIgnoreCase) == 0)
                continue;
            if (++errors > 1)
                return errors;
            i--;
        }

        return errors;
    }


    [GeneratedRegex(@"<span>(\w+)</span>")]
    private static partial Regex ResultRegex { get; }
}