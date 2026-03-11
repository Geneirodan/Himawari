using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink.Entities;
using Geneirodan.MediatR.Abstractions;
using Himawari.Discord.Music.Abstractions;
using MediatR;

namespace Himawari.Discord.Music.Commands;

/// <summary>Discord slash command: return up to the first 100 tracks in the queue (for display).</summary>
public sealed record QueueCommand(BaseContext Context) : PlayerCommandBase(Context), ICommand<LavalinkTrack[]>
{
    /// <inheritdoc />
    public sealed class Handler : IRequestHandler<QueueCommand, Result<LavalinkTrack[]>>
    {
        public Task<Result<LavalinkTrack[]>> Handle(
            QueueCommand request,
            CancellationToken cancellationToken
        ) => Task.FromResult(Result<LavalinkTrack[]>.Success(request.Player.Queue.Take(100).ToArray()));
    }
}