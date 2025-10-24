using Himawari.Telegram.Core.Abstractions.Messages;
using JetBrains.Annotations;

namespace Himawari.Telegram.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(ICommand))]
public sealed class BotCommandAttribute(string command) : Attribute
{
    public string Command { get; } = command;
}