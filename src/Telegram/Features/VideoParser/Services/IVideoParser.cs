using Ardalis.Result;
using Telegram.Bot.Types;

namespace Himawari.VideoParser.Services;

/// <summary>
/// Parses a media URL (e.g. video link) and returns Telegram album input media for sending to a chat.
/// </summary>
public interface IVideoParser
{
    /// <summary>
    /// Fetches or processes the URL and returns a list of media items suitable for an album, or an error result.
    /// </summary>
    /// <param name="url">Source URL (e.g. video or audio).</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result is a <see cref="Result{T}"/> with album media or errors.</returns>
    Task<Result<IAlbumInputMedia[]>> GetInputFiles(string url, CancellationToken token = default);
    /// <summary>Returns whether the text contains a URL that this parser can handle.</summary>
    bool ContainsUrl(string url);
}