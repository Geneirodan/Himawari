using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Geneirodan.Mediator;
using Himawari.Discord.Music.Abstractions;
using Mediator;

namespace Himawari.Discord.Music.Behaviors;

public sealed class CurrentTrackCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICurrentTrackCommand
    where TResponse : class, IResult
{
    private readonly ErrorList _errorList = new(["No track playing rn"]);

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var player = message.Player;
        if (player.CurrentTrack is not { } currentTrack)
            return DynamicResults.Error<TResponse>(_errorList);

        message.CurrentTrack = currentTrack;
        return await next(message, cancellationToken).ConfigureAwait(false);
    }
}
