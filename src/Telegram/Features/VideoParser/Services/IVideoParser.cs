using Ardalis.Result;
using Telegram.Bot.Types;

namespace Himawari.VideoParser.Services;

public interface IVideoParser
{
    Task<Result<IAlbumInputMedia[]>> GetInputFiles(string url, CancellationToken token = default);
    bool ContainsUrl(string url);
}