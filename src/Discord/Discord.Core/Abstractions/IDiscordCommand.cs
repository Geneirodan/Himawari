using DisCatSharp.ApplicationCommands.Context;

namespace Himawari.Discord.Core.Abstractions;

/// <summary>
/// Contract for a Discord application command that exposes the invocation <see cref="Context"/>.
/// </summary>
public interface IDiscordCommand
{
    /// <summary>The Discord application command context (guild, channel, user, options).</summary>
    BaseContext Context { get; }
}