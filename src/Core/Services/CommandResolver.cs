using System.Globalization;
using Himawari.Core.Abstractions;
using Himawari.Core.Abstractions.Messages;
using Himawari.Core.Models;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Himawari.Core.Services;

public sealed class CommandResolver : ICommandResolver
{
    private readonly Dictionary<string, string> _commandsByAlias;
    private readonly Dictionary<string, Func<Message, string, ICommand>> _commandFactories;
    private readonly ICommandDescriptor[] _descriptors;

    public CommandResolver(IEnumerable<ICommandDescriptor> descriptors, IOptionsMonitor<Aliases> aliases)
    {
        _descriptors = descriptors.ToArray();
        _commandsByAlias = [];
        foreach (var (command, aliasSet) in aliases.CurrentValue)
        foreach (var alias in aliasSet)
            if (!_commandsByAlias.TryAdd(alias, command))
                throw new ArgumentException($"Duplicate alias {alias}");

        _commandFactories = _descriptors.ToDictionary(x => x.Keyword, x => x.Factory);
    }

    public Func<Message, string, ICommand>? GetFactoryByName(string commandName) =>
        _commandFactories.GetValueOrDefault(commandName);

    public string? GetCommandByAlias(string alias) => _commandsByAlias.GetValueOrDefault(alias) ??
                                                      _commandFactories.Keys.FirstOrDefault(x => x == alias);

    public IEnumerable<BotCommand> GetCommandsByCulture(CultureInfo cultureInfo)
    {
        var oldCulture = Thread.CurrentThread.CurrentUICulture;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        var commands = _descriptors.Select(x => new BotCommand(x.Keyword, x.Description));
        Thread.CurrentThread.CurrentUICulture = oldCulture;
        return commands;
    }
}