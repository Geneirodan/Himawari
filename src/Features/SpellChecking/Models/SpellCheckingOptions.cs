namespace Himawari.SpellChecking.Models;

public sealed record SpellCheckingOptions
{
    public double Threshold { get; init; } = 0.5;
    public required string LayoutsFilePath { get; init; }
    public required string DictionariesPath { get; init; }
    public string? AccPath { get; init; }
}