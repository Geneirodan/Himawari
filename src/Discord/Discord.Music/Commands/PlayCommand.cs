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

namespace Himawari.Discord.Music.Commands;

public sealed record PlayCommand(BaseContext Context, string Query) : PlayerCommandBase(Context), ICommand<string>
{
    public sealed class Handler : IRequestHandler<PlayCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(PlayCommand request, CancellationToken cancellationToken)
        {
            var query = request.Query;
            var connection = request.Player;

            var result = await connection.LoadTracksAsync(query).ConfigureAwait(false);

            string name;
            switch (result.LoadType)
            {
                case LavalinkLoadResultType.Track:
                    var track = result.GetResultAs<LavalinkTrack>();
                    connection.AddToQueue(track);
                    name = track.CreateTrackName();
                    break;
                case LavalinkLoadResultType.Playlist:
                    var playlist = result.GetResultAs<LavalinkPlaylist>();
                    connection.AddToQueue(playlist);
                    name = playlist.Info.Name;
                    break;
                case LavalinkLoadResultType.Search:
                    var lavalinkTrack = result.GetResultAs<List<LavalinkTrack>>()[0];
                    connection.AddToQueue(lavalinkTrack);
                    name = lavalinkTrack.CreateTrackName();
                    break;
                case LavalinkLoadResultType.Empty:
                case LavalinkLoadResultType.Error:
                default:
                    return Result.Error($"Track search failed for `{query}`.");
            }

            if (connection.CurrentTrack is null)
                connection.PlayQueue();

            var builder = new StringBuilder()
                .Append('`')
                .Append(name)
                .Append("` added to queue! ");
            if (connection.Queue is { Count: > 0 and var count })
                builder.Append("Now there is ")
                    .Append(count.ToString(CultureInfo.InvariantCulture))
                    .Append(" songs in queue.");

            return Result.Success(builder.ToString());
        }
    }

    public override bool ShouldAutoConnect => true;
}