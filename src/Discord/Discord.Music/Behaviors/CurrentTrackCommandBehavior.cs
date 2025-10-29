using Ardalis.Result;
using Geneirodan.MediatR;
using Himawari.Discord.Music.Abstractions;
using MediatR;

namespace Himawari.Discord.Music.Behaviors;

public sealed class CurrentTrackCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICurrentTrackCommand
    where TResponse : class, IResult
{
    private readonly ErrorList _errorList = new(["No track playing rn"]);
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var player = request.Player;
        if (player.CurrentTrack is not { } currentTrack)
            return DynamicResults.Error<TResponse>(_errorList);

        request.CurrentTrack = currentTrack;
        return await next(cancellationToken).ConfigureAwait(false);

    }

}