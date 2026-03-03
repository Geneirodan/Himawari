using JetBrains.Annotations;

namespace Himawari.SpellChecking.Models;

/// <summary>
/// Defines a keyboard layout: character rows for standard and shift states (used for wrong-layout mapping).
/// </summary>
[UsedImplicitly]
public sealed record KeyboardLayout
{
    /// <summary>Characters in standard (non-shift) order.</summary>
    public required string[] Standard { get; init; }
    /// <summary>Characters in shift order.</summary>
    public required string[] Shift { get; init; }
}