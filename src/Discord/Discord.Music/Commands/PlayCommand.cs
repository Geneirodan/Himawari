using System.Globalization;
using System.Text;
using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using Geneirodan.MediatR.Abstractions;
using Himawari.Discord.Music.Abstractions;
using Himawari.Discord.Music.Extensions;
using MediatR;
using static Himawari.Discord.Music.Commands.PlayCommand;

namespace Himawari.Discord.Music.Commands;

public sealed record PlayCommand(BaseContext Context, string Query) : PlayerCommandBase(Context), ICommand<Response>
{
    public sealed record Response(string TrackName, int QueueCount);
    public sealed class Handler : IRequestHandler<PlayCommand, Result<Response>>
    {
        public async Task<Result<Response>> Handle(PlayCommand request, CancellationToken cancellationToken)
        {
            var query = request.Query;
            var connection = request.Player;

            var result = await connection.LoadTracksAsync(query).ConfigureAwait(false);

            string trackName;
            switch (result.LoadType)
            {
                case LavalinkLoadResultType.Track:
                    var track = result.GetResultAs<LavalinkTrack>();
                    connection.AddToQueue(track);
                    trackName = track.CreateTrackName();
                    break;
                case LavalinkLoadResultType.Playlist:
                    var playlist = result.GetResultAs<LavalinkPlaylist>();
                    connection.AddToQueue(playlist);
                    trackName = playlist.Info.Name;
                    break;
                case LavalinkLoadResultType.Search:
                    var lavalinkTrack = result.GetResultAs<List<LavalinkTrack>>()[0];
                    connection.AddToQueue(lavalinkTrack);
                    trackName = lavalinkTrack.CreateTrackName();
                    break;
                case LavalinkLoadResultType.Empty:
                case LavalinkLoadResultType.Error:
                default:
                    return Result.Error($"Track search failed for `{query}`.");
            }

            if (connection.CurrentTrack is null)
                connection.PlayQueue();

            return new Response(trackName, connection.Queue.Count);
        }
    }

    public override bool ShouldAutoConnect => true;
}