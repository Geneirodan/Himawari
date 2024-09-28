using System.Runtime.InteropServices;
using DeepL;
using Himawari.Abstractions.Services;
using Himawari.Options;
using Microsoft.Extensions.Options;

namespace Himawari.Services;

public class AliasService(HttpClient client, IOptions<ApiOptions> options, ILogger<AliasService> logger) : IAliasService
{
    private readonly Dictionary<long, HashSet<int>> _messages = [];
    private readonly Dictionary<long, long?> _presenterIds = [];
    private readonly Dictionary<long, string?> _words = [];

    private async Task<string?> GetNewWordAsync(CancellationToken cancellationToken = default)
    {
        var msg = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(options.Value.WordsUrl)
        };
        using var response = await client.SendAsync(msg, cancellationToken).ConfigureAwait(false);
        var words = await response.Content
            .ReadFromJsonAsync<IEnumerable<string>>(cancellationToken)
            .ConfigureAwait(false);
        return words?.FirstOrDefault();
    }

    private async Task<string> TranslateWordAsync(string word, string twoLetterIsoLanguageName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await new Translator(options.Value.DeepLApiKey)
                .TranslateTextAsync(
                    word,
                    LanguageCode.English,
                    twoLetterIsoLanguageName,
                    cancellationToken: cancellationToken
                );
            return result.Text;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to translate random word");
        }

        return word;
    }

    private async Task<string?> GetNewTranslatedWordAsync(CancellationToken cancellationToken = default)
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        var word = await GetNewWordAsync(cancellationToken).ConfigureAwait(false);
        if (word is not null && culture is { TwoLetterISOLanguageName: { } isoName and not LanguageCode.English })
            word = await TranslateWordAsync(word, isoName, cancellationToken).ConfigureAwait(false);
        return word;
    }

    public async Task<string?> GetCurrentWordAsync(long chatId, CancellationToken cancellationToken = default)
    {
        // ref var word = ref CollectionsMarshal.GetValueRefOrAddDefault(_words, chatId, out var exists);
        //  if (!exists)
        //      word = await GetNewTranslatedWordAsync(cancellationToken).ConfigureAwait(false);
        //  return word;
        if (_words.TryGetValue(chatId, out var word))
            return word;
        word = await GetNewTranslatedWordAsync(cancellationToken).ConfigureAwait(false);
        _words[chatId] = word;
        return word;
    }

    public void Restart(long chatId)
    {
        ResetWord(chatId);
        _presenterIds.Remove(chatId);
    }

    public long? GetPresenterId(long chatId) => _presenterIds.GetValueOrDefault(chatId);
    public void SetPresenterId(long chatId, long presenterId) => _presenterIds[chatId] = presenterId;

    public HashSet<int> GetMessages(long chatId)
    {
        ref var messages = ref CollectionsMarshal.GetValueRefOrAddDefault(_messages, chatId, out var exists);
        if (!exists)
            messages = [];
        return messages!;
    }

    public void ResetWord(long chatId) => _words.Remove(chatId);
}