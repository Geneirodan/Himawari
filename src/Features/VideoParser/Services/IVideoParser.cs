using Ardalis.Result;
using Telegram.Bot.Types;

namespace Himawari.VideoParser.Services;

public interface IVideoParser
{
    Task<Result<InputFile>> GetInputFile(string url);
    bool ContainsUrl(string url);
    string Type { get; }
}