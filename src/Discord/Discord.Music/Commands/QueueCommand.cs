using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink.Entities;
using Himawari.Discord.Music.Abstractions;
using Mediator;

namespace Himawari.Discord.Music.Commands;

public sealed record QueueCommand(BaseContext Context) : PlayerCommandBase(Context), ICommand<Result<LavalinkTrack[]>>
{
    public sealed class Handler : ICommandHandler<QueueCommand, Result<LavalinkTrack[]>>
    {
        public ValueTask<Result<LavalinkTrack[]>> Handle(QueueCommand request, CancellationToken cancellationToken) =>
            ValueTask.FromResult<Result<LavalinkTrack[]>>(request.Player.Queue.Take(100).ToArray());
    }
}
