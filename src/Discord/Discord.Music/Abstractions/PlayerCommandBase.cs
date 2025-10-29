using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink;

namespace Himawari.Discord.Music.Abstractions;

public abstract record PlayerCommandBase(BaseContext Context) : IPlayerCommand
{
    public virtual LavalinkGuildPlayer Player { get; set; } = null!;
    public virtual bool ShouldAutoConnect => false;
}