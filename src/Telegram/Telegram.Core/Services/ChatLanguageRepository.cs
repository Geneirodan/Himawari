using Himawari.Telegram.Core.Localization;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;

namespace Himawari.Telegram.Core.Services;

/// <summary>
/// SQLite-backed language repository with IMemoryCache L1 cache.
/// Schema: Chats (Id INTEGER PRIMARY KEY, Lang TEXT NOT NULL). Cache TTL: 1 hour (refreshed on write).
/// </summary>
public sealed class ChatLanguageRepository(SqliteConnection connection, IMemoryCache cache) : ILanguageRepository
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    /// <inheritdoc />
    public async Task<string?> GetAsync(long chatId, CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey(chatId), out string? cached))
            return cached;

        var command = new SqliteCommand("SELECT Lang FROM Chats WHERE Id = @Id;", connection);
        command.Parameters.AddWithValue("@Id", chatId);
        var lang = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) as string;

        if (lang is not null)
            cache.Set(CacheKey(chatId), lang, CacheTtl);

        return lang;
    }

    /// <inheritdoc />
    public async Task SetAsync(long chatId, string languageCode, CancellationToken cancellationToken = default)
    {
        var command = new SqliteCommand(
            """
            INSERT INTO Chats (Id, Lang) VALUES (@Id, @Lang)
            ON CONFLICT(Id) DO UPDATE SET Lang = excluded.Lang;
            """,
            connection);
        command.Parameters.AddWithValue("@Id", chatId);
        command.Parameters.AddWithValue("@Lang", languageCode);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        cache.Set(CacheKey(chatId), languageCode, CacheTtl);
    }

    private static string CacheKey(long chatId) => $"chat:lang:{chatId}";
}
