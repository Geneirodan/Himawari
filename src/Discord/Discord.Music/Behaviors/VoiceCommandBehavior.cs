using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using DisCatSharp.Lavalink;
using Geneirodan.Mediator;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using Mediator;

namespace Himawari.Discord.Music.Behaviors;

public sealed class VoiceCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IPlayerCommand
    where TResponse : class, IResult
{
    private static TResponse CreateErrorResult(params IEnumerable<string> errorMessages)
    {
        var errorList = new ErrorList(errorMessages);
        return DynamicResults.Error<TResponse>(errorList);
    }

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var lavaResult = message.Context.GetLavalink();
        if (!lavaResult.IsSuccess)
            return CreateErrorResult(lavaResult.Errors);

        var node = lavaResult.Value.ConnectedSessions.Values.First();
        var voiceState = message.Context.Member?.VoiceState;

        if (voiceState?.Channel is null)
            return CreateErrorResult("You must be connected to a voice channel to use this command!");

        var connection = node.GetGuildPlayer(voiceState.Guild!);

        if (connection is null)
        {
            const string errorMessage = "The bot is not connected to the voice channel in this guild!";
            if (!message.ShouldAutoConnect)
                return CreateErrorResult(errorMessage);
            await voiceState.Channel.ConnectAsync(node).ConfigureAwait(false);
            connection = node.GetGuildPlayer(voiceState.Guild!);
            if (connection is null)
                return CreateErrorResult(errorMessage);
        }
        else if (voiceState.Channel != connection.Channel)
            return CreateErrorResult("You must be in the same voice channel as the bot!");

        message.Player = connection;
        return await next(message, cancellationToken).ConfigureAwait(false);
    }
}
