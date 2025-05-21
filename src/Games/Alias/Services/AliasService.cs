using System.Globalization;
using System.Text.RegularExpressions;

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

    public string? GetCurrentWord(long chatId) => Words.GetValueOrDefault(chatId);

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
            {"qu_words", "1"},
            {"type_words", "objects"},
            {"order", "in_order"},
            {"done", "Create"}
        };
        var formContent = new FormUrlEncodedContent(nameValueCollection);
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var requestUri = $"https://teoset.com/word-generator/lang.{culture}#element_list";
        using (var response = await client.PostAsync(requestUri, formContent, cancellationToken).ConfigureAwait(false))
        {
            var str = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Words[chatId] =  ResultRegex.Matches(str).First().Groups[1].Value.Trim();
        }
        return Words[chatId];
    }

    public async Task<string> GetOrCreateCurrentWordAsync(long chatId, CancellationToken cancellationToken = default) => 
        GetCurrentWord(chatId) ?? await NextWordAsync(chatId, cancellationToken).ConfigureAwait(false);


    [GeneratedRegex(@"<span>(\w+)</span>")]
    private static partial Regex ResultRegex { get; }
}