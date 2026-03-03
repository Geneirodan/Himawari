using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Geneirodan.MediatR.Abstractions;
using Himawari.Discord.Music.Abstractions;
using MediatR;

namespace Himawari.Discord.Music.Commands;

/// <summary>Discord slash command: stop playback, clear the queue, and disconnect the bot from the voice channel.</summary>
public sealed record StopCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand
{
    /// <inheritdoc />
    public sealed class Handler : IRequestHandler<StopCommand, Result>
    {
        public async Task<Result> Handle(StopCommand request, CancellationToken cancellationToken)
        {
            request.Player.ClearQueue();
            await request.Player.StopAsync().ConfigureAwait(false);
            await request.Player.DisconnectAsync().ConfigureAwait(false);
            return Result.Success();
        }
    }
}