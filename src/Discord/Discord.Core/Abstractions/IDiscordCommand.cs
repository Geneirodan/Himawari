using DisCatSharp.ApplicationCommands.Context;

namespace Himawari.Discord.Core.Abstractions;

public interface IDiscordCommand
{
    BaseContext Context { get; }
}