using Himawari.Telegram.Core.Abstractions.Messages;
using JetBrains.Annotations;

namespace Himawari.Telegram.Core.Attributes;

/// <summary>
/// Marks a class as a Telegram bot command and specifies the command keyword (e.g. "start"). Used by <see cref="AbstractCommandDescriptor{TCommand}"/> to build the command list.
/// </summary>
/// <param name="command">Command keyword without leading slash (e.g. "start" for /start).</param>
[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(ICommand))]
public sealed class BotCommandAttribute(string command) : Attribute
{
    /// <summary>The command keyword (with or without leading slash).</summary>
    public string Command { get; } = command;
}