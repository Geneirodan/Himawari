using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.VideoParser.Replies;
using Himawari.VideoParser.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Himawari.VideoParser;

/// <summary>
/// Message handler: finds URLs in text via registered <see cref="IVideoParser"/> instances, calls <see cref="IVideoParser.GetInputFiles"/> and sends <see cref="ParseVideoReply"/> on success or <see cref="ErrorReply"/> on failure.
/// </summary>
[PublicAPI]
public sealed partial class VideoParsingDispatcher(
    IServiceProvider serviceProvider,
    ILogger<VideoParsingDispatcher> logger
) : AbstractDispatcher
{
    /// <inheritdoc />
    protected override async Task OnNewMessage(Message msg)
    {
        if (msg.Text is not { } messageText)
            return;

        var scope = serviceProvider.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var parsers = scope.ServiceProvider.GetServices<IVideoParser>();
            foreach (var parser in parsers)
            {
                if (!parser.ContainsUrl(url: messageText)) continue;
                LogDetectedUrl(messageText);
                var file = await parser.GetInputFiles(messageText).ConfigureAwait(false);
                if (file.IsSuccess)
                {
                    var stream = await sender.Send(new ParseVideoReply(msg, file.Value)).ConfigureAwait(false);
                    await foreach (var _ in stream.ConfigureAwait(false))
                    { }
                }
                else
                {
                    await sender.Send(new ErrorReply(msg, file.Errors.First())).ConfigureAwait(false);
                }
            }
        }
    }

    [LoggerMessage(LogLevel.Information, "Detected video url in message '{Message}'")]
    private partial void LogDetectedUrl(string message);
}