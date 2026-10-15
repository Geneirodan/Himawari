using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace Himawari.Discord.Music.Extensions;

public static class BaseContextExtensions
{
    extension(BaseContext ctx)
    {
        public Result<LavalinkExtension> GetLavalink()
        {
            var lava = ctx.Client.GetLavalink();

            if (lava.ConnectedSessions.Any())
                return lava;

            return Result.Error("The Lavalink connection is not established!");
        }

        public async Task CreateResponseWithContent(string content, bool asEphemeral = false)
        {
            var response = new DiscordInteractionResponseBuilder().WithContent(content);
            if (asEphemeral)
                response = response.AsEphemeral();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response)
                .ConfigureAwait(false);
        }

        public async Task EditResponseWithContent(string content)
        {
            var withContent = new DiscordWebhookBuilder().WithContent(content);
            await ctx.EditResponseAsync(withContent).ConfigureAwait(false);
        }
    }
}
