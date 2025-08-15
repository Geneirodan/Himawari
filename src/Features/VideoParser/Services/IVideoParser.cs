using Ardalis.Result;
using Telegram.Bot.Types;

namespace Himawari.VideoParser.Services;

public interface IVideoParser
{
    Task<Result<IAlbumInputMedia[]>> GetInputFiles(string url);
    bool ContainsUrl(string url);
    string Type { get; }
}