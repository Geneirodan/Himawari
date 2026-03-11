using JetBrains.Annotations;

// ReSharper disable CollectionNeverUpdated.Global

namespace Himawari.SpellChecking.Models;

/// <summary>
/// Root model for keyboard layout configuration: named layouts and locale-to-layouts mapping.
/// </summary>
[UsedImplicitly]
public sealed record LayoutSettings
{
    /// <summary>Map of layout name to <see cref="KeyboardLayout"/>.</summary>
    public required IDictionary<string, KeyboardLayout> Layouts { get; init; }
    /// <summary>Map of locale to layout names.</summary>
    public required IDictionary<string, string[]> Locales { get; init; }
}