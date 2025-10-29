using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Geneirodan.MediatR.Abstractions;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using MediatR;

namespace Himawari.Discord.Music.Commands;

public sealed record SkipCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand<string>
{
    public sealed class Handler : IRequestHandler<SkipCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(SkipCommand request, CancellationToken cancellationToken)
        {
            var trackName = request.CurrentTrack.CreateTrackName();
            await request.Player.SkipAsync().ConfigureAwait(false);
            return trackName;
        }
    }
}