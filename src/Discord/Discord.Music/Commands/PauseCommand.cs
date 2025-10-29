using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Geneirodan.MediatR.Abstractions;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using MediatR;

namespace Himawari.Discord.Music.Commands;

public sealed record PauseCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand<string>
{
    public sealed class Handler : IRequestHandler<PauseCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(PauseCommand request, CancellationToken cancellationToken)
        {
            var trackName = request.CurrentTrack.CreateTrackName();
            await request.Player.PauseAsync().ConfigureAwait(false);
            return Result<string>.Success(trackName);
        }
    }
}