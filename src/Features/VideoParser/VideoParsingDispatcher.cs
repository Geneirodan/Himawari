using Himawari.Core.Abstractions;
using Himawari.Core.Abstractions.Messages;
using Himawari.VideoParser.Replies;
using Himawari.VideoParser.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.VideoParser;

[PublicAPI]
public sealed class VideoParsingDispatcher(IServiceProvider serviceProvider) : IMessageHandler
{
    public async Task OnMessage(Message msg, UpdateType update)
    {
        if (msg.Text is not { } messageText) return;

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