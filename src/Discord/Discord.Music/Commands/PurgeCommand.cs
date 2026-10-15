using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Himawari.Discord.Music.Abstractions;
using Mediator;

namespace Himawari.Discord.Music.Commands;

public sealed record PurgeCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand<Result>
{
    public sealed class Handler : ICommandHandler<PurgeCommand, Result>
    {
        public async ValueTask<Result> Handle(PurgeCommand request, CancellationToken cancellationToken)
        {
            request.Player.ClearQueue();
            await request.Player.StopAsync().ConfigureAwait(false);
            return Result.Success();
        }
    }
}
