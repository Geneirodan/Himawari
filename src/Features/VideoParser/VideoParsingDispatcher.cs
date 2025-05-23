﻿using Himawari.Core.Abstractions;
using Himawari.Core.Abstractions.Messages;
using Himawari.VideoParser.Replies;
using Himawari.VideoParser.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Himawari.VideoParser;

[PublicAPI]
public sealed partial class VideoParsingDispatcher(IServiceProvider serviceProvider, ILogger<VideoParsingDispatcher> logger) : AbstractDispatcher
{
    protected override async Task OnNewMessage(Message msg)
    {
        if (msg.Text is not { } messageText) 
            return;

        using var scope = serviceProvider.CreateScope();

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var parsers = scope.ServiceProvider.GetServices<IVideoParser>();
        foreach (var parser in parsers)
        {
            if (!parser.ContainsUrl(url: messageText)) continue;
            LogDetectedUrl(parser.Type, messageText);
            var file = await parser.GetInputFile(messageText).ConfigureAwait(false);
            IReply reply = file.IsSuccess
                ? new ParseVideoReply(msg, file.Value)
                : new ErrorReply(msg, file.Errors.First());
            await sender.Send(reply).ConfigureAwait(false);
        }
    }
    
    [LoggerMessage(LogLevel.Information, "Detected {Type} url in message '{Message}'")]
    private partial void LogDetectedUrl(string type, string message);
}