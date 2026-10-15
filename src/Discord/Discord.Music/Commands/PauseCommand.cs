using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using Mediator;

namespace Himawari.Discord.Music.Commands;

public sealed record PauseCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand<Result<string>>
{
    public sealed class Handler : ICommandHandler<PauseCommand, Result<string>>
    {
        public async ValueTask<Result<string>> Handle(PauseCommand request, CancellationToken cancellationToken)
        {
            var trackName = request.CurrentTrack.CreateTrackName();
            await request.Player.PauseAsync().ConfigureAwait(false);
            return trackName;
        }
    }
}
