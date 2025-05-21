namespace Himawari.Alias.Services;

public interface IAliasService
{
    Task<string> StartAsync(long chatId, long presenterId, CancellationToken cancellationToken = default);
    string? GetCurrentWord(long chatId);
    void EndGame(long chatId);
    long? GetPresenterId(long chatId);
    Task<string> NextWordAsync(long chatId, CancellationToken cancellationToken = default);
    Task<string> GetOrCreateCurrentWordAsync(long chatId, CancellationToken cancellationToken = default);
}