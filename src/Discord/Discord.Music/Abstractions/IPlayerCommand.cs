using DisCatSharp.Lavalink;
using Himawari.Discord.Core.Abstractions;

namespace Himawari.Discord.Music.Abstractions;

/// <summary>
/// Discord command that uses a Lavalink guild player; may require auto-connect to voice before execution.
/// </summary>
public interface IPlayerCommand : IDiscordCommand
{
    /// <summary>Lavalink player for the guild.</summary>
    LavalinkGuildPlayer Player { get; set; }
    /// <summary>Whether the command should ensure the bot is connected to voice before running.</summary>
    bool ShouldAutoConnect { get; }
}