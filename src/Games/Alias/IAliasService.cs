namespace Himawari.Alias;

public interface IAliasService
{
    Task<string?> GetCurrentWordAsync(long chatId, CancellationToken cancellationToken = default);
    void Restart(long chatId);
    long? GetPresenterId(long chatId);
    HashSet<int> GetMessages(long chatId);
    void ResetWord(long chatId);
    void SetPresenterId(long chatId, long presenterId);
}