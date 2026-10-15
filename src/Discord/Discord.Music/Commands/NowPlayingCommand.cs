using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using Mediator;

namespace Himawari.Discord.Music.Commands;

public sealed record NowPlayingCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand<Result<string>>
{
    public sealed class Handler : ICommandHandler<NowPlayingCommand, Result<string>>
    {
        public ValueTask<Result<string>> Handle(NowPlayingCommand request, CancellationToken cancellationToken) =>
            ValueTask.FromResult<Result<string>>(request.CurrentTrack.CreateTrackName());
    }
}
