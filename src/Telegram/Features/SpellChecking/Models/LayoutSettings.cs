using JetBrains.Annotations;

// ReSharper disable CollectionNeverUpdated.Global

namespace Himawari.SpellChecking.Models;

[UsedImplicitly]
public sealed record LayoutSettings
{
    public required Dictionary<string, KeyboardLayout> Layouts { get; init; }
    public required Dictionary<string, string[]> Locales { get; init; }
}