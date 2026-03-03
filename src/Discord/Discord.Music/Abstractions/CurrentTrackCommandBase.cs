using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink.Entities;

namespace Himawari.Discord.Music.Abstractions;

/// <summary>
/// Base type for Discord commands that operate on the current track (e.g. skip, now playing). <see cref="ICurrentTrackCommand.CurrentTrack"/> is set by <see cref="CurrentTrackCommandBehavior{TRequest,TResponse}"/> before the handler runs.
/// </summary>
/// <param name="Context">The Discord application command context.</param>
public abstract record CurrentTrackCommandBase(BaseContext Context) : PlayerCommandBase(Context), ICurrentTrackCommand
{
    /// <inheritdoc />
    public virtual LavalinkTrack CurrentTrack { get; set; } = null!;
}