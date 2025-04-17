using System.Globalization;
using System.Text.RegularExpressions;

namespace Himawari.Alias.Services;

public partial class AliasService(HttpClient client) : IAliasService
{
    private readonly Dictionary<long, long?> _presenterIds = [];
    private readonly Dictionary<long, string?> _words = [];

    public async Task<string?> GetCurrentWordAsync(long chatId, CancellationToken cancellationToken = default)
    {
        if (_words.TryGetValue(chatId, out var word))
            return word;
        word = await GetNewWordAsync(cancellationToken).ConfigureAwait(false);
        _words[chatId] = word;
        return word;
    }

    public void Restart(long chatId)
    {
        ResetWord(chatId);
        _presenterIds.Remove(chatId);
    }

    public long? GetPresenterId(long chatId)
    {
        return _presenterIds.GetValueOrDefault(chatId);
    }

    public void SetPresenterId(long chatId, long presenterId)
    {
        _presenterIds[chatId] = presenterId;
    }

    public void ResetWord(long chatId)
    {
        _words.Remove(chatId);
    }


    private async Task<string?> GetNewWordAsync(CancellationToken cancellationToken = default)
    {
        var formContent = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("qu_words", "1"),
            new KeyValuePair<string, string>("type_words", "objects"),
            new KeyValuePair<string, string>("order", "in_order"),
            new KeyValuePair<string, string>("done", "Create")
        ]);
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var requestUri = $"https://teoset.com/word-generator/lang.{culture}#element_list";
        using var response = await client.PostAsync(requestUri, formContent, cancellationToken).ConfigureAwait(false);
        var str = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var first = ResultRegex().Matches(str).First();
        var word = first.Groups[1].Value.Trim();
        return word;
    }

    [GeneratedRegex("""<span>(\w+)</span>""")]
    private static partial Regex ResultRegex();
}