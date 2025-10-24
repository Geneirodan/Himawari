using Himawari.Alias.Enums;

namespace Himawari.Alias.Services;

public interface IAliasService
{
    Task<string> StartAsync(long chatId, long presenterId, CancellationToken cancellationToken = default);
    void EndGame(long chatId);
    long? GetPresenterId(long chatId);
    Task<string> NextWordAsync(long chatId, CancellationToken cancellationToken = default);
    Task<string> GetOrCreateCurrentWordAsync(long chatId, CancellationToken cancellationToken = default);
    
    Guess VerifyWord(long chatId, string word);
}