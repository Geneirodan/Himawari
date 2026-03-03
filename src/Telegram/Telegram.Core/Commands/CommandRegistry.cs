using System.Buffers;
using System.Collections.Frozen;
using System.Globalization;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.Options;
using Himawari.Telegram.Core.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Commands;

/// <summary>
/// Immutable registry of all bot commands, built once at startup. Provides O(1) case-insensitive exact lookup,
/// alias resolution, and Levenshtein-based fuzzy matching for typo suggestions ("Did you mean?").
/// Implements <see cref="ICommandResolver"/> for backward compatibility.
/// </summary>
public sealed class CommandRegistry : ICommandResolver
{
    private const int MaxLevenshteinBufferLength = 64;

    private readonly FrozenDictionary<string, CommandEntry> _commands;
    private readonly FrozenDictionary<string, string> _aliases;
    private readonly FrozenDictionary<string, string> _naturalLanguageLookup;
    private readonly ICommandDescriptor[] _descriptors;
    private readonly string[] _allCanonicalNames;
    private readonly CommandRegistryOptions _options;

    /// <summary>
    /// Builds the registry from descriptors and alias configuration.
    /// </summary>
    /// <param name="descriptors">All registered command descriptors.</param>
    /// <param name="aliases">Alias map from configuration (e.g. Telegram:Aliases).</param>
    /// <param name="options">Registry options (fuzzy distance, show suggestions, natural-language triggers).</param>
    public CommandRegistry(
        IEnumerable<ICommandDescriptor> descriptors,
        IOptionsMonitor<Aliases> aliases,
        IOptions<CommandRegistryOptions> options)
    {
        _options = options.Value;
        _descriptors = descriptors.ToArray();

        var commands = new Dictionary<string, CommandEntry>(StringComparer.OrdinalIgnoreCase);
        var aliasesMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var desc in _descriptors)
        {
            var canonical = desc.Keyword.TrimStart('/');
            var entry = new CommandEntry(desc.Keyword, desc.Description, desc.Factory);
            commands[canonical] = entry;

            foreach (var alias in desc.Aliases)
            {
                var aliasKey = alias.TrimStart('/');
                if (!aliasesMap.TryAdd(aliasKey, canonical))
                    throw new ArgumentException($"Duplicate alias '{alias}'", nameof(aliases));
            }
        }

        // Merge config aliases (keyword -> set of aliases) into alias -> canonical
        foreach (var (keyword, aliasSet) in aliases.CurrentValue)
        {
            var canonical = keyword.TrimStart('/');
            if (!commands.ContainsKey(canonical))
                continue;
            foreach (var alias in aliasSet)
            {
                var aliasKey = alias.TrimStart('/');
                aliasesMap.TryAdd(aliasKey, canonical);
            }
        }

        _commands = commands.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        _aliases = aliasesMap.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        _allCanonicalNames = commands.Keys.ToArray();

        var triggers = _options.NaturalLanguageTriggers;
        if (triggers is { Count: > 0 })
        {
            // Keys are regex patterns (e.g. "(?i)\b(help|хелп)\b"); do not use case-insensitive comparer.
            var nlDict = new Dictionary<string, string>(triggers.Count, StringComparer.Ordinal);
            foreach (var kv in triggers)
                nlDict[kv.Key] = kv.Value;
            _naturalLanguageLookup = nlDict.ToFrozenDictionary(StringComparer.Ordinal);
        }
        else
            _naturalLanguageLookup = FrozenDictionary<string, string>.Empty;
    }

    /// <summary>
    /// Resolves input to a command using multi-tier matching: exact → alias → fuzzy (Levenshtein).
    /// </summary>
    /// <param name="input">Command token without leading slash (e.g. "help", "hlep").</param>
    /// <param name="maxDistance">Maximum Levenshtein distance for fuzzy suggestions (default from options).</param>
    /// <returns>Match result: Exact, Alias, Fuzzy (with suggestions), or NotFound.</returns>
    public CommandMatchResult Resolve(ReadOnlySpan<char> input, int? maxDistance = null)
    {
        var key = input.Trim().ToString();
        if (string.IsNullOrEmpty(key))
            return CommandMatchResult.NotFound(key);

        var distance = maxDistance ?? _options.FuzzyMaxDistance;

        if (_commands.TryGetValue(key, out var exact))
            return CommandMatchResult.Exact(exact);

        if (_aliases.TryGetValue(key, out var canonical) && _commands.TryGetValue(canonical, out var aliased))
            return CommandMatchResult.Alias(aliased, key);

        var suggestions = FindSuggestions(key, distance);
        return suggestions.Length > 0
            ? CommandMatchResult.Fuzzy(suggestions)
            : CommandMatchResult.NotFound(key);
    }

    /// <inheritdoc />
    public Func<Message, string, ICommand>? GetFactoryByName(string commandName)
    {
        var canonical = commandName?.TrimStart('/');
        if (string.IsNullOrEmpty(canonical))
            return null;
        return _commands.TryGetValue(canonical, out var entry) ? entry.Factory : null;
    }

    /// <inheritdoc />
    public string? GetCommandByAlias(string alias)
    {
        var key = alias?.TrimStart('/');
        if (string.IsNullOrEmpty(key))
            return null;
        if (_aliases.TryGetValue(key, out var canonical) && _commands.TryGetValue(canonical, out var entry))
            return entry.Keyword;
        if (_commands.TryGetValue(key, out entry))
            return entry.Keyword;
        return null;
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
            return _descriptors.Select(x => new BotCommand(x.Keyword, x.Description));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = oldCulture;
            Thread.CurrentThread.CurrentUICulture = oldUiCulture;
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> GetAlternateLookup() => _naturalLanguageLookup;

    private string[] FindSuggestions(string input, int maxDistance)
    {
        var list = new List<(string name, int distance)>();
        var buffer = ArrayPool<int>.Shared.Rent(MaxLevenshteinBufferLength);
        try
        {
            var span = buffer.AsSpan(0, MaxLevenshteinBufferLength);
            foreach (var name in _allCanonicalNames)
            {
                var distance = LevenshteinDistance(input.AsSpan(), name.AsSpan(), span);
                if (distance <= maxDistance)
                    list.Add((name, distance));
            }
            return list
                .OrderBy(x => x.distance)
                .ThenBy(x => x.name.Length)
                .Take(3)
                .Select(x => x.name)
                .ToArray();
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Span-based Levenshtein with single-row DP. Trims common prefix/suffix to reduce work. Uses stackalloc buffer when lengths allow.
    /// </summary>
    private static int LevenshteinDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target, Span<int> buffer)
    {
        if (source.IsEmpty) return target.Length;
        if (target.IsEmpty) return source.Length;

        int start = 0;
        while (start < source.Length && start < target.Length
               && char.ToLowerInvariant(source[start]) == char.ToLowerInvariant(target[start]))
            start++;

        int sourceEnd = source.Length, targetEnd = target.Length;
        while (sourceEnd > start && targetEnd > start
               && char.ToLowerInvariant(source[sourceEnd - 1]) == char.ToLowerInvariant(target[targetEnd - 1]))
        {
            sourceEnd--;
            targetEnd--;
        }

        source = source[start..sourceEnd];
        target = target[start..targetEnd];

        if (source.IsEmpty) return target.Length;
        if (target.IsEmpty) return source.Length;

        if (target.Length > buffer.Length)
            return int.MaxValue;

        var costs = buffer[..target.Length];
        for (var j = 0; j < costs.Length; j++)
            costs[j] = j + 1;

        for (var i = 0; i < source.Length; i++)
        {
            var sc = char.ToLowerInvariant(source[i]);
            var corner = i;
            costs[0] = char.ToLowerInvariant(target[0]) == sc ? i : i + 1;

            for (var j = 1; j < target.Length; j++)
            {
                var upper = costs[j];
                costs[j] = char.ToLowerInvariant(target[j]) == sc
                    ? corner
                    : 1 + Math.Min(Math.Min(upper, costs[j - 1]), corner);
                corner = upper;
            }
        }

        return costs[target.Length - 1];
    }
}
