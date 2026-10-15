using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using Mediator;
using static Himawari.Discord.Music.Commands.SkipCommand;

namespace Himawari.Discord.Music.Commands;

public sealed record SkipCommand(BaseContext Context) : CurrentTrackCommandBase(Context), ICommand<Result<Response>>
{
    public sealed record Response(string OldTrackName, string? NewTrackName, int QueueCount);

    public sealed class Handler : ICommandHandler<SkipCommand, Result<Response>>
    {
        public async ValueTask<Result<Response>> Handle(SkipCommand request, CancellationToken cancellationToken)
        {
            var trackName = request.CurrentTrack.CreateTrackName();
            var nextTrackName = request.Player.Queue[0].CreateTrackName();
            await request.Player.SkipAsync().ConfigureAwait(false);
            return new Response(trackName, nextTrackName, request.Player.Queue.Count);
        }
    }
}
