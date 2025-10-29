using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink.Entities;

namespace Himawari.Discord.Music.Abstractions;

public abstract record CurrentTrackCommandBase(BaseContext Context) : PlayerCommandBase(Context), ICurrentTrackCommand
{
    public virtual LavalinkTrack CurrentTrack { get; set; } = null!;
}