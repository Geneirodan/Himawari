using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink;
using Geneirodan.MediatR.Abstractions;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using MediatR;

namespace Himawari.Discord.Music.Commands;

public sealed record StopCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand
{
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