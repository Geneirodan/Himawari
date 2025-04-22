using Telegram.Bot.Types;
using YoutubeExplode.Videos.Streams;

namespace Himawari.VideoParser.Services;

public interface IVideoParser
{
    Task<InputFile?> GetInputFile(string url);
    bool ContainsUrl(string url);
    string Type { get; }
}