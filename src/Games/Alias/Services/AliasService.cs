using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Himawari.Alias.Enums;

namespace Himawari.Alias.Services;

public partial class AliasService(HttpClient client) : IAliasService
{
    private static readonly Dictionary<long, long?> PresenterIds = [];
    private static readonly Dictionary<long, string> Words = [];

    public async Task<string> StartAsync(long chatId, long presenterId, CancellationToken cancellationToken = default)
    {
        PresenterIds[chatId] = presenterId;
        return await NextWordAsync(chatId, cancellationToken).ConfigureAwait(false);
    }

    public void EndGame(long chatId)
    {
        Words.Remove(chatId);
        PresenterIds.Remove(chatId);
    }

    public long? GetPresenterId(long chatId) => PresenterIds.GetValueOrDefault(chatId);

    public async Task<string> NextWordAsync(long chatId, CancellationToken cancellationToken = default)
    {
        var nameValueCollection = new Dictionary<string, string>
        {
            { "qu_words", "1" },
            { "type_words", "objects" },
            { "order", "in_order" },
            { "done", "Create" }
        };
        var formContent = new FormUrlEncodedContent(nameValueCollection);
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var requestUri = $"https://teoset.com/word-generator/lang.{culture}#element_list";
        using (var response = await client.PostAsync(requestUri, formContent, cancellationToken).ConfigureAwait(false))
        {
            var str = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Words[chatId] = ResultRegex.Matches(str).First().Groups[1].Value.Trim();
        }

        return Words[chatId];
    }

    public async Task<string> GetOrCreateCurrentWordAsync(long chatId, CancellationToken cancellationToken = default) =>
        Words.GetValueOrDefault(chatId) ?? await NextWordAsync(chatId, cancellationToken).ConfigureAwait(false);

    public Guess VerifyWord(long chatId, string word)
    {
        var current = Words.GetValueOrDefault(chatId);
        if (current is null)
            return Guess.Incorrect;

        var delta = current.Length - word.Length;
        
        var errors = delta switch
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