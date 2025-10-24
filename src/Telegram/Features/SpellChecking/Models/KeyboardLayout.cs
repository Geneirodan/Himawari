using JetBrains.Annotations;

namespace Himawari.SpellChecking.Models;

[UsedImplicitly]
public sealed record KeyboardLayout
{
    public required string[] Standard { get; init; }
    public required string[] Shift { get; init; }
}