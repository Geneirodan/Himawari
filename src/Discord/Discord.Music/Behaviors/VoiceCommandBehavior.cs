using Ardalis.Result;
using DisCatSharp.Lavalink;
using Geneirodan.MediatR;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using MediatR;

namespace Himawari.Discord.Music.Behaviors;

public sealed class VoiceCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IPlayerCommand
    where TResponse : class, IResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var lavaResult = request.Context.GetLavalink();
        if (!lavaResult.IsSuccess)
            return CreateErrorResult(lavaResult.Errors);
        
        var node = lavaResult.Value.ConnectedSessions.Values.First();
        var voiceState = request.Context.Member?.VoiceState;

        if (voiceState?.Channel is null)
            return CreateErrorResult("You must be connected to a voice channel to use this command!");
        
        ArgumentNullException.ThrowIfNull(voiceState.Guild);

        var connection = node.GetGuildPlayer(voiceState.Guild);
        
        if (connection is null)
        {
            const string errorMessage = "The bot is not connected to the voice channel in this guild!";
            if (!request.ShouldAutoConnect) 
                return CreateErrorResult(errorMessage);
            await voiceState.Channel.ConnectAsync(node).ConfigureAwait(false);
            connection = node.GetGuildPlayer(voiceState.Guild);
            if (connection is null)
                return CreateErrorResult(errorMessage);
        }
        else if (voiceState.Channel != connection.Channel)
            return CreateErrorResult("You must be in the same voice channel as the bot!");

        request.Player = connection;
        return await next(cancellationToken).ConfigureAwait(false);
    }

    private static TResponse CreateErrorResult(params IEnumerable<string> errorMessages)
    {
        var errorList = new ErrorList(errorMessages);
        return DynamicResults.Error<TResponse>(errorList);
    }
}