using Telegram.Bot.Types;

namespace Himawari.VideoParser.Services;

public interface IVideoParser
{
    Task<InputFile?> GetInputFile(string url);
    bool ContainsUrl(string url);
    string Type { get; }
}