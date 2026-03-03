namespace Himawari.SillyThings.Options;

/// <summary>
/// Configuration for the SillyThings feature: URLs for rhino GIF and SS sticker assets, and trigger phrases (substrings for sticker, exact match for GIF).
/// </summary>
public sealed record SillyThingsOptions
{
    /// <summary>URL of the rhino GIF to send in response to GIF triggers.</summary>
    public required string RhinoGifUrl { get; init; }

    /// <summary>URL of the SS sticker.</summary>
    public required string SsStickerUrl { get; init; }

    /// <summary>Phrases that trigger the sticker reply when the message <em>contains</em> any of them (ordinal). Default: <c>["SS"]</c>.</summary>
    public IList<string> StickerTriggers { get; init; } = ["SS"];

    /// <summary>Phrases that trigger the rhino GIF when the message <em>equals</em> any of them (case-insensitive). Default: <c>["какіш"]</c>.</summary>
    public IList<string> GifTriggers { get; init; } = ["какіш"];
}