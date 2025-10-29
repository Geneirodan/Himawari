using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Geneirodan.MediatR.Abstractions;
using Himawari.Discord.Music.Abstractions;
using MediatR;

namespace Himawari.Discord.Music.Commands;

public sealed record PurgeCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand
{
    public sealed class Handler : IRequestHandler<PurgeCommand, Result>
    {
        public async Task<Result> Handle(PurgeCommand request, CancellationToken cancellationToken)
        {
            request.Player.ClearQueue();
            await request.Player.StopAsync().ConfigureAwait(false);
            return Result.Success();
        }
    }
}