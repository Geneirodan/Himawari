using JetBrains.Annotations;

// ReSharper disable CollectionNeverUpdated.Global

namespace Himawari.SpellChecking.Models;

[UsedImplicitly]
public sealed record LayoutSettings
{
    public required IDictionary<string, KeyboardLayout> Layouts { get; init; }
    public required IDictionary<string, string[]> Locales { get; init; }
}