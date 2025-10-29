using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Geneirodan.MediatR.Abstractions;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using MediatR;

namespace Himawari.Discord.Music.Commands;

public sealed record ResumeCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand<string>
{
    public sealed class Handler : IRequestHandler<ResumeCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(ResumeCommand request, CancellationToken cancellationToken)
        {
            await request.Player.ResumeAsync().ConfigureAwait(false);
            return request.CurrentTrack.CreateTrackName();
        }
    }
}