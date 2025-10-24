using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace Himawari.Discord.Music;

public static class BaseContextExtensions
{
    public static async Task<Result<LavalinkGuildPlayer>> GetConnection(this BaseContext ctx, bool autoConnect = false)
    {
        var lavaResult = ctx.GetLavalink();
        if (!lavaResult.IsSuccess)
            return Result.Error(lavaResult.Errors.First());
        var node = lavaResult.Value.ConnectedSessions.Values.First();
        var voiceState = ctx.Member?.VoiceState;

        if (voiceState?.Channel is null)
            return Result.Error("You must be connected to a voice channel to use this command!");

        ArgumentNullException.ThrowIfNull(voiceState.Guild);

        var connection = node.GetGuildPlayer(voiceState.Guild);


        if (connection is null)
        {
            const string errorMessage = "The bot is not connected to the voice channel in this guild!";
            if (!autoConnect) 
                return Result.Error(errorMessage);
            await voiceState.Channel.ConnectAsync(node).ConfigureAwait(false);
            connection = node.GetGuildPlayer(voiceState.Guild);
            if (connection is null)
                return Result.Error(errorMessage);
        }

        if (voiceState.Channel == connection.Channel)
            return connection;

        return Result.Error("You must be in the same voice channel as the bot!");
    }


    public static Result<LavalinkExtension> GetLavalink(this BaseContext ctx)
    {
        var lava = ctx.Client.GetLavalink();

        if (lava.ConnectedSessions.Any())
            return lava;

        return Result.Error("The Lavalink connection is not established!");
    }

    public static async Task CreateResponseWithContent(this BaseContext ctx, string content, bool asEphemeral = false)
    {
        var response = new DiscordInteractionResponseBuilder()
            .WithContent(content);
        if (asEphemeral)
            response = response.AsEphemeral();
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response).ConfigureAwait(false);
    }
    
    
    public static async Task EditResponseWithContent(this BaseContext ctx, string content)
    {
        var withContent = new DiscordWebhookBuilder().WithContent(content);
        await ctx.EditResponseAsync(withContent).ConfigureAwait(false);
    }
}