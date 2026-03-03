using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Geneirodan.MediatR.Abstractions;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using MediatR;

namespace Himawari.Discord.Music.Commands;

/// <summary>Discord slash command: return the display name of the currently playing track.</summary>
public sealed record NowPlayingCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand<string>
{
    /// <inheritdoc />
    public sealed class Handler : IRequestHandler<NowPlayingCommand, Result<string>>
    {
        public Task<Result<string>> Handle(NowPlayingCommand request, CancellationToken cancellationToken) =>
            Task.FromResult<Result<string>>(request.CurrentTrack.CreateTrackName());
    }
}