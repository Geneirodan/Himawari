namespace Himawari.VideoParser.Options;

public sealed record VideoParsingOptions
{
    public required string CobaltToolsUrl { get; init; }
}