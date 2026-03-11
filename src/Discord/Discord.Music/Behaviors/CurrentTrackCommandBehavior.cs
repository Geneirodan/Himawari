using Ardalis.Result;
using Geneirodan.MediatR;
using Himawari.Discord.Music.Abstractions;
using MediatR;

namespace Himawari.Discord.Music.Behaviors;

/// <summary>
/// MediatR behavior for <see cref="ICurrentTrackCommand"/>: ensures a track is currently playing, assigns <see cref="ICurrentTrackCommand.CurrentTrack"/> to the request, or returns an error result.
/// </summary>
/// <typeparam name="TRequest">The current-track command type.</typeparam>
/// <typeparam name="TResponse">The response type (must implement <see cref="IResult"/>).</typeparam>
public sealed class CurrentTrackCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICurrentTrackCommand
    where TResponse : class, IResult
{
    private readonly ErrorList _errorList = new(["No track playing rn"]);

    /// <inheritdoc />
    /// <param name="request">The current-track command; <see cref="ICurrentTrackCommand.CurrentTrack"/> is set when a track is playing.</param>
    /// <param name="next">The next delegate in the pipeline (the handler).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the handler response, or an error result when no track is playing (does not throw).</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var player = request.Player;
        if (player.CurrentTrack is not { } currentTrack)
            return DynamicResults.Error<TResponse>(_errorList);

        request.CurrentTrack = currentTrack;
        return await next(cancellationToken).ConfigureAwait(false);
    }

}