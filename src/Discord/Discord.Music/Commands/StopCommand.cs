using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Himawari.Discord.Music.Abstractions;
using Mediator;

namespace Himawari.Discord.Music.Commands;

public sealed record StopCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand<Result>
{
    public sealed class Handler : ICommandHandler<StopCommand, Result>
    {
        public async ValueTask<Result> Handle(StopCommand request, CancellationToken cancellationToken)
        {
            request.Player.ClearQueue();
            await request.Player.StopAsync().ConfigureAwait(false);
            await request.Player.DisconnectAsync().ConfigureAwait(false);
            return Result.Success();
        }
    }
}
