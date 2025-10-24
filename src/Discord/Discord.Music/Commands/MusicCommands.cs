using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using JetBrains.Annotations;

namespace Himawari.Discord.Music.Commands;

[PublicAPI]
public class MusicCommands : ApplicationCommandsModule
{
    [ContextMenu(ApplicationCommandType.Message, "Play")]
    public static Task PlayAsync(ContextMenuContext ctx) => PlayInnerAsync(ctx, ctx.TargetMessage.Content);
    
    [SlashCommand("play", "Play music asynchronously")]
    public static Task PlayAsync(InteractionContext ctx, [Option("query", "Search string or link")] string query)
        => PlayInnerAsync(ctx, query);

    private static async Task PlayInnerAsync(BaseContext ctx, string query)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
        var connectionResult = await ctx.GetConnection(true).ConfigureAwait(false);

        if (!connectionResult.IsSuccess)
        {
            await ctx.EditResponseWithContent(connectionResult.Errors.First()).ConfigureAwait(false);
            return;
        }

        var connection = connectionResult.Value;

        var result = await connection.LoadTracksAsync(query).ConfigureAwait(false);

        string name;
        switch (result.LoadType)
        {
            case LavalinkLoadResultType.Track:
                var track = result.GetResultAs<LavalinkTrack>();
                connection.AddToQueue(track);
                name = track.Info.Title;
                break;
            case LavalinkLoadResultType.Playlist:
                var playlist = result.GetResultAs<LavalinkPlaylist>();
                connection.AddToQueue(playlist);
                name = playlist.Info.Name;
                break;
            case LavalinkLoadResultType.Search:
                var lavalinkTrack = result.GetResultAs<List<LavalinkTrack>>().First();
                connection.AddToQueue(lavalinkTrack);
                name = lavalinkTrack.Info.Title;
                break;
            case LavalinkLoadResultType.Empty:
            case LavalinkLoadResultType.Error:
            default:
                await ctx.EditResponseWithContent($"Track search failed for {query}.").ConfigureAwait(false);
                return;
        }

        if (connection.CurrentTrack is null)
            connection.PlayQueue();

        await ctx.EditResponseWithContent($"{name} added to queue!").ConfigureAwait(false);
    }


    [SlashCommand("pause", "Pause playback")]
    public static async Task PauseAsync(InteractionContext ctx)
    {
        var connectionResult = await ctx.GetConnection().ConfigureAwait(false);

        if (!connectionResult.IsSuccess)
        {
            await ctx.CreateResponseWithContent(connectionResult.Errors.First(), true).ConfigureAwait(false);
            return;
        }

        var connection = connectionResult.Value;

        await connection.PauseAsync().ConfigureAwait(false);

        await ctx.CreateResponseWithContent($"Paused `{connection.CurrentTrack?.Info.Title}`").ConfigureAwait(false);
    }

    [SlashCommand("resume", "Resume playback")]
    public static async Task ResumeAsync(InteractionContext ctx)
    {
        var connectionResult = await ctx.GetConnection().ConfigureAwait(false);

        if (!connectionResult.IsSuccess)
        {
            await ctx.CreateResponseWithContent(connectionResult.Errors.First(), true).ConfigureAwait(false);
            return;
        }

        var connection = connectionResult.Value;

        await connection.ResumeAsync().ConfigureAwait(false);

        await ctx.CreateResponseWithContent($"Now playing `{connection.CurrentTrack?.Info.Title}`").ConfigureAwait(false);
    }

    [SlashCommand("stop", "Stop playback")]
    public static async Task StopAsync(InteractionContext ctx)
    {
        var connectionResult = await ctx.GetConnection().ConfigureAwait(false);

        if (!connectionResult.IsSuccess)
        {
            await ctx.CreateResponseWithContent(connectionResult.Errors.First(), true).ConfigureAwait(false);
            return;
        }

        var connection = connectionResult.Value;

        await connection.StopAsync().ConfigureAwait(false);

        await ctx.CreateResponseWithContent("Playback is stopped!").ConfigureAwait(false);
    }
    [SlashCommand("skip", "Skip song")]
    public static async Task SkipAsync(InteractionContext ctx)
    {
        var connectionResult = await ctx.GetConnection().ConfigureAwait(false);

        if (!connectionResult.IsSuccess)
        {
            await ctx.CreateResponseWithContent(connectionResult.Errors.First(), true).ConfigureAwait(false);
            return;
        }

        var connection = connectionResult.Value;

        await connection.SkipAsync().ConfigureAwait(false);

        await ctx.CreateResponseWithContent($"Skipped `{connection.CurrentTrack?.Info.Title}`").ConfigureAwait(false);
    }

}
