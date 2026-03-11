using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink;

namespace Himawari.Discord.Music.Abstractions;

/// <summary>
/// Base type for Discord music commands that use a <see cref="LavalinkGuildPlayer"/>. The player is set by <see cref="VoiceCommandBehavior{TRequest,TResponse}"/> before the handler runs; derived commands should not assign <see cref="Player"/> manually.
/// </summary>
/// <param name="Context">The Discord application command context.</param>
/// <remarks>Override <see cref="ShouldAutoConnect"/> to return <see langword="true"/> when the command may start playback and the bot should connect to voice if not already connected.</remarks>
public abstract record PlayerCommandBase(BaseContext Context) : IPlayerCommand
{
    /// <inheritdoc />
    public virtual LavalinkGuildPlayer Player { get; set; } = null!;
    /// <inheritdoc />
    public virtual bool ShouldAutoConnect => false;
}