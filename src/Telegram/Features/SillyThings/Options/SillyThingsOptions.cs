namespace Himawari.SillyThings.Options;

public sealed record SillyThingsOptions
{
    public required string RhinoGifUrl { get; init; }
    public required string SsStickerUrl { get; init; }
}