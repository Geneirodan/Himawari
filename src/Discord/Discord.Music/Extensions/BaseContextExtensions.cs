using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace Himawari.Discord.Music.Extensions;

public static class BaseContextExtensions
{
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