using System.Reflection;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Models;
using JetBrains.Annotations;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions;

/// <summary>
/// Base implementation of <see cref="ICommandDescriptor"/> that reads the command keyword from <see cref="BotCommandAttribute"/> on <typeparamref name="TCommand"/> and resolves aliases from <see cref="Aliases"/>.
/// </summary>
/// <typeparam name="TCommand">The command type (must have <see cref="BotCommandAttribute"/>).</typeparam>
[UsedImplicitly]
public abstract class AbstractCommandDescriptor<TCommand> : ICommandDescriptor where TCommand : ICommand
{
    /// <summary>
    /// Initializes the descriptor from <paramref name="aliases"/> and the <see cref="BotCommandAttribute"/> on <typeparamref name="TCommand"/>.
    /// </summary>
    /// <param name="aliases">Alias map from configuration.</param>
    /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="TCommand"/> has no <see cref="BotCommandAttribute"/>.</exception>
    protected AbstractCommandDescriptor(Aliases aliases)
    {
        var type = typeof(TCommand);
        var attribute = type.GetCustomAttributes<BotCommandAttribute>().FirstOrDefault()
                        ?? throw new InvalidOperationException(
                            $"Command {type.Name} has no {nameof(BotCommandAttribute)} attribute");
        Keyword = $"/{attribute.Command.TrimStart('/')}";
        Aliases = aliases.GetValueOrDefault(Keyword) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public abstract string Description { get; }
    /// <inheritdoc />
    public string Keyword { get; }
    /// <inheritdoc />
    public abstract Func<Message, string, ICommand> Factory { get; }
    /// <inheritdoc />
    public IReadOnlySet<string> Aliases { get; }
}