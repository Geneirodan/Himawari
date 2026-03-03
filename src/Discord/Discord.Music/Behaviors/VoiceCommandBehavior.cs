using Ardalis.Result;
using DisCatSharp.Lavalink;
using Geneirodan.MediatR;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using MediatR;

namespace Himawari.Discord.Music.Behaviors;

/// <summary>
/// MediatR behavior for <see cref="IPlayerCommand"/>: ensures the user is in a voice channel, resolves or creates the Lavalink guild player, assigns <see cref="IPlayerCommand.Player"/> to the request, and optionally auto-connects the bot.
/// </summary>
/// <typeparam name="TRequest">The player command type.</typeparam>
/// <typeparam name="TResponse">The response type (must implement <see cref="IResult"/>).</typeparam>
public sealed class VoiceCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IPlayerCommand
    where TResponse : class, IResult
{
    /// <inheritdoc />
    /// <param name="request">The player command; <see cref="IPlayerCommand.Player"/> is assigned when the user is in a voice channel and the bot is connected.</param>
    /// <param name="next">The next delegate in the pipeline (the handler).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the handler response, or an error result when Lavalink is unavailable, the user is not in voice, or the bot is not in the same channel (does not throw for those cases).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> member's voice state or its guild is <see langword="null"/> despite being in a channel — indicates an unexpected Discord API state.</exception>
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

        var guild = voiceState?.Guild ?? throw new ArgumentNullException(nameof(request), "Member's voice state or its guild is null.");

        var connection = node.GetGuildPlayer(guild);

        if (connection is null)
        {
            const string errorMessage = "The bot is not connected to the voice channel in this guild!";
            if (!request.ShouldAutoConnect)
                return CreateErrorResult(errorMessage);
            await voiceState.Channel.ConnectAsync(node).ConfigureAwait(false);
            connection = node.GetGuildPlayer(guild);
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