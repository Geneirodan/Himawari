using DisCatSharp.ApplicationCommands.Context;
using Mediator;

namespace Himawari.Discord.Core.Abstractions;

public interface IDiscordCommand : IBaseCommand
{
    BaseContext Context { get; }
}
