﻿using System.Reflection;
using Himawari.Core.Abstractions.Messages;
using Himawari.Core.Attributes;
using Himawari.Core.Models;
using JetBrains.Annotations;
using Telegram.Bot.Types;

namespace Himawari.Core.Abstractions;

[UsedImplicitly]
public abstract class AbstractCommandDescriptor<TCommand> : ICommandDescriptor where TCommand : ICommand
{
    protected AbstractCommandDescriptor(Aliases aliases)
    {
        var type = typeof(TCommand);
        var attribute = type.GetCustomAttributes<BotCommandAttribute>().FirstOrDefault()
                        ?? throw new InvalidOperationException(
                            $"Command {type.Name} has no {nameof(BotCommandAttribute)} attribute");
        Keyword = $"/{attribute.Command.TrimStart('/')}";
        Aliases = aliases.GetValueOrDefault(Keyword) ?? new HashSet<string>();
    }

    public abstract string Description { get; }
    public string Keyword { get; }
    public abstract Func<Message, string, ICommand> Factory { get; }
    public IReadOnlySet<string> Aliases { get; }
}