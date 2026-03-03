using System.ComponentModel.DataAnnotations;

namespace Himawari.SpellChecking.Models;

/// <summary>
/// Configuration for spell checking and wrong-layout detection: file paths for layouts and dictionaries, optional threshold and .acc path.
/// </summary>
public sealed record SpellCheckingOptions
{
    /// <summary>Confidence threshold for layout detection (default 0.5).</summary>
    public double Threshold { get; init; } = 0.5;

    /// <summary>Path to the YAML file containing keyboard layout definitions.</summary>
    [Required]
    public required string LayoutsFilePath { get; init; }

    /// <summary>Path to Hunspell dictionary files.</summary>
    [Required]
    public required string DictionariesPath { get; init; }
    /// <summary>Optional path to .acc file.</summary>
    public string? AccPath { get; init; }

    /// <summary>When <see langword="true"/>, sends a "Maybe, you meant this?" reply when wrong keyboard layout is detected. Default <see langword="false"/> to avoid noisy or incorrect suggestions.</summary>
    public bool SendWrongLayoutReply { get; init; }
}