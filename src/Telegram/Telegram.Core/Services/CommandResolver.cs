using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Commands;
using Himawari.Telegram.Core.Models;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Services;

/// <summary>
/// Resolves command factories by name or alias and builds the bot command list per culture from registered <see cref="ICommandDescriptor"/> instances. Implements <see cref="ICommandResolver"/>.
/// </summary>
public sealed class CommandResolver : ICommandResolver
{
    private readonly Dictionary<string, Func<Message, string, ICommand>> _commandFactories;
    private readonly Dictionary<string, string> _commandsByAlias;
    private readonly ICommandDescriptor[] _descriptors;

    public CommandResolver(IEnumerable<ICommandDescriptor> descriptors, IOptionsMonitor<Aliases> aliases)
    {
        _descriptors = descriptors.ToArray();
        _commandsByAlias = [];
        foreach (var (command, aliasSet) in aliases.CurrentValue)
        foreach (var alias in aliasSet)
            if (!_commandsByAlias.TryAdd(alias, command))
                throw new ArgumentException($"Duplicate alias {alias}", nameof(aliases));

        _commandFactories = _descriptors.ToDictionary(x => x.Keyword, x => x.Factory, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Func<Message, string, ICommand>? GetFactoryByName(string commandName)
    {
        return _commandFactories.GetValueOrDefault(commandName);
    }

    /// <inheritdoc />
    public string? GetCommandByAlias(string alias)
    {
        return _commandsByAlias.GetValueOrDefault(alias) ??
               _commandFactories.Keys.FirstOrDefault(x => string.Equals(x, alias, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public IEnumerable<BotCommand> GetCommandsByCulture(CultureInfo cultureInfo)
    {
        var oldCulture = Thread.CurrentThread.CurrentCulture;
        var oldUiCulture = Thread.CurrentThread.CurrentUICulture;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        try
        {
            return _descriptors.Select(x => new BotCommand(x.Keyword, x.Description)).ToList();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = oldCulture;
            Thread.CurrentThread.CurrentUICulture = oldUiCulture;
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> GetAlternateLookup() => FrozenDictionary<string, string>.Empty;

    /// <inheritdoc />
    /// <remarks>
    /// Legacy stub — implements only Exact/Alias matching without Fuzzy.
    /// Use <see cref="CommandRegistry"/> (registered as <see cref="ICommandResolver"/> in DI).
    /// </remarks>
    [Obsolete("Use CommandRegistry via DI. This stub exists only for interface compliance.")]
    public CommandMatchResult Resolve(ReadOnlySpan<char> input, int? maxDistance = null)
    {
        var key = input.Trim().ToString();
        if (string.IsNullOrEmpty(key))
            return CommandMatchResult.NotFound(key);
        var withSlash = key.StartsWith('/') ? key : $"/{key}";
        var factory = GetFactoryByName(withSlash);
        if (factory is not null)
        {
            var entry = new CommandEntry(withSlash, _descriptors.First(d => string.Equals(d.Keyword, withSlash, StringComparison.OrdinalIgnoreCase)).Description, factory);
            return CommandMatchResult.Exact(entry);
        }
        var canonical = GetCommandByAlias(withSlash);
        if (canonical is not null)
        {
            var f = GetFactoryByName(canonical)!;
            var desc = _descriptors.First(d => string.Equals(d.Keyword, canonical, StringComparison.OrdinalIgnoreCase));
            return CommandMatchResult.Alias(new CommandEntry(canonical, desc.Description, f), key);
        }
        return CommandMatchResult.NotFound(key);
    }
}