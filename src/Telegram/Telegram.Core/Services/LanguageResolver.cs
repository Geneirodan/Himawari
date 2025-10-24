using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;

namespace Himawari.Telegram.Core.Services;

public sealed class LanguageResolver(IMemoryCache cache, SqliteConnection connection) : ILanguageResolver
{
    public async Task<CultureInfo> GetCurrentCulture(long chatId) => cache.Get<CultureInfo>(chatId) 
                                                                     ?? await GetLanguage(chatId).ConfigureAwait(false);

    private async Task<CultureInfo> GetLanguage(long chatId)
    {
        var command = new SqliteCommand("SELECT Lang FROM Chats WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", chatId);
        var current = await command.ExecuteScalarAsync().ConfigureAwait(false) as string ?? "en";
        var culture = new CultureInfo(current);
        cache.Set(chatId, culture);
        return culture;
    }
}