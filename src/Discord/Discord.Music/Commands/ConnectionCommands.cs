using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink;
using JetBrains.Annotations;

namespace Himawari.Discord.Music.Commands;

[PublicAPI]
public sealed class ConnectionCommands : ApplicationCommandsModule
{
    [SlashCommand("connect", "Join the voice channel")]
    public static async Task ConnectAsync(InteractionContext ctx)
    {
        var lava = ctx.GetLavalink();

        if (!lava.IsSuccess)
        {
            await ctx.CreateResponseWithContent(lava.Errors.First()).ConfigureAwait(false);
            return;
        }

        var voiceState = ctx.Member?.VoiceState;
        if (voiceState is null)
        {
            await ctx.CreateResponseWithContent("You must be connected to a voice channel to use this command!", asEphemeral: true)
                .ConfigureAwait(false);
            return;
        }


        var channel = voiceState.Channel!;
        await channel.ConnectAsync(lava.Value.ConnectedSessions.Values.First()).ConfigureAwait(false);

        await ctx.CreateResponseWithContent($"The bot has joined the channel {channel.Name.InlineCode()}")
            .ConfigureAwait(false);
    }


    [SlashCommand("leave", "Leave the voice channel")]
    public static async Task LeaveAsync(InteractionContext ctx)
    {
        var lava = ctx.GetLavalink();

        if (!lava.IsSuccess)
        {
            await ctx.CreateResponseWithContent(lava.Errors.First(), asEphemeral: true).ConfigureAwait(false);
            return;
        }

        ArgumentNullException.ThrowIfNull(ctx.Guild);

        var connection = lava.Value.ConnectedSessions.Values.First().GetGuildPlayer(ctx.Guild);

        if (connection is null)
        {
            await ctx.CreateResponseWithContent("The bot is not connected to the voice channel in this guild!", asEphemeral: true)
                .ConfigureAwait(false);
            return;
        }

        await connection.DisconnectAsync().ConfigureAwait(false);

        await ctx.CreateResponseWithContent("The bot left the voice channel").ConfigureAwait(false);
    }

}