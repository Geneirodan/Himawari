using Himawari.Core;
using Himawari.Core.Abstractions.Messages;
using Himawari.VideoParser.Replies;
using Himawari.VideoParser.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Himawari.VideoParser;

[PublicAPI]
public sealed class VideoParsingDispatcher(IServiceProvider serviceProvider) : AbstractDispatcher
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
            var file = await parser.GetInputFile(messageText).ConfigureAwait(false);
            IReply reply = file.IsSuccess
                ? new ParseVideoReply(msg, file.Value)
                : new ErrorReply(msg, file.Errors.First());
            await sender.Send(reply).ConfigureAwait(false);
        }
    }
}