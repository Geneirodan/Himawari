namespace Himawari.Telegram.Core.Models;

/// <summary>
/// Maps command keywords to sets of alias strings. Loaded from configuration (e.g. Telegram:Aliases) and used by <see cref="AbstractCommandDescriptor{TCommand}"/> to resolve aliases per command.
/// </summary>
public sealed class Aliases : Dictionary<string, IReadOnlySet<string>>;