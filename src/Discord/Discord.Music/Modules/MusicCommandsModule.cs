using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Enums;
using Himawari.Discord.Music.Commands;
using Himawari.Discord.Music.Extensions;
using JetBrains.Annotations;
using MediatR;

namespace Himawari.Discord.Music.Modules;

[PublicAPI]
public sealed class MusicCommandsModule(ISender sender) : ApplicationCommandsModule
{

    [SlashCommand("play", "Play or enqueue a track")]
    public async Task PlayAsync(InteractionContext ctx, [Option("query", "Search string or link")] string query)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
        var command = new PlayCommand(ctx, query);
        var result = await sender.Send(command).ConfigureAwait(false);
        await result.HandleResult(
            async () => await ctx.EditResponseWithContent(result.Value).ConfigureAwait(false),
            async () => await ctx.EditResponseWithContent(result.Errors.First()).ConfigureAwait(false)
        ).ConfigureAwait(false);
    }


    [SlashCommand("pause", "Pause playback")]
    public async Task PauseAsync(InteractionContext ctx)
    {
        var command = new PauseCommand(ctx);
        var result = await sender.Send(command).ConfigureAwait(false);
        await result.HandleResult(
            ctx,
            async () => await ctx.CreateResponseWithContent($"Paused `{result.Value}`").ConfigureAwait(false)
        ).ConfigureAwait(false);
    }


    [SlashCommand("queue", "Show first 100 queued tracks")]
    public async Task QueueAsync(InteractionContext ctx)
    {
        var command = new QueueCommand(ctx);
        var result = await sender.Send(command).ConfigureAwait(false);
        await result.HandleResult(
            ctx,
            async () => await ctx.CreateResponseWithContent(result.Value).ConfigureAwait(false)
        ).ConfigureAwait(false);
    }


    [SlashCommand("now_playing", "Show current playing track")]
    public async Task NowPlayingAsync(InteractionContext ctx)
    {
        var command = new NowPlayingCommand(ctx);
        var result = await sender.Send(command).ConfigureAwait(false);
        await result.HandleResult(
            ctx,
            async () => await ctx.CreateResponseWithContent($"Now playing `{result.Value}`", asEphemeral: true)
                .ConfigureAwait(false)
        ).ConfigureAwait(false);
    }


    [SlashCommand("resume", "Resume playback")]
    public async Task ResumeAsync(InteractionContext ctx)
    {
        var command = new NowPlayingCommand(ctx);
        var result = await sender.Send(command).ConfigureAwait(false);
        await result.HandleResult(
            ctx,
            async () => await ctx.CreateResponseWithContent($"Now playing `{result.Value}`", asEphemeral: true)
                .ConfigureAwait(false)
        ).ConfigureAwait(false);
    }

    [SlashCommand("stop", "Stop playback")]
    public async Task StopAsync(InteractionContext ctx)
    {
        var command = new StopCommand(ctx);
        var result = await sender.Send(command).ConfigureAwait(false);
        await result.HandleResult(
            ctx,
            async () => await ctx.CreateResponseWithContent("Playback is stopped!").ConfigureAwait(false)
        ).ConfigureAwait(false);
    }

    [SlashCommand("purge", "Purge da queue")]
    public async Task PurgeAsync(InteractionContext ctx)
    {
        var command = new PurgeCommand(ctx);
        var result = await sender.Send(command).ConfigureAwait(false);
        await result.HandleResult(
            ctx,
            async () => await ctx.CreateResponseWithContent("Queue is purged!").ConfigureAwait(false)
        ).ConfigureAwait(false);
    }

    [SlashCommand("skip", "Skip song")]
    public async Task SkipAsync(InteractionContext ctx)
    {
        var command = new NowPlayingCommand(ctx);
        var result = await sender.Send(command).ConfigureAwait(false);
        await result.HandleResult(
            ctx,
            async () => await ctx.CreateResponseWithContent($"Skipped `{result.Value}`").ConfigureAwait(false)
        ).ConfigureAwait(false);
        
    }
}