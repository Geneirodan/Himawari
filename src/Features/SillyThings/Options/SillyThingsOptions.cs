namespace Himawari.SillyThings.Options;

public record SillyThingsOptions
{
    public required string RhinoGifUrl { get; init; }
    public required string SsGifUrl { get; init; }
}