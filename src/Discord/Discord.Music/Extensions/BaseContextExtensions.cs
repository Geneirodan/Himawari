using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace Himawari.Discord.Music.Extensions;

/// <summary>
/// Extension methods for Discord <see cref="BaseContext"/>: get Lavalink (with connection check) and create/edit interaction responses.
/// </summary>
public static class BaseContextExtensions
{
    /// <summary>Gets the Lavalink extension from the client, or an error result if no session is connected.</summary>
    /// <param name="ctx">The command context.</param>
    /// <returns>A success result with the extension, or an error if Lavalink is not connected.</returns>
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
    
    
    /// <summary>Edits the deferred interaction response with the given content.</summary>
    public static async Task EditResponseWithContent(this BaseContext ctx, string content)
    {
        var withContent = new DiscordWebhookBuilder().WithContent(content);
        await ctx.EditResponseAsync(withContent).ConfigureAwait(false);
    }
}