namespace Himawari.Telegram.Core.Html;

/// <summary>
/// Marker type for pre-encoded or trusted HTML content. Use only for trusted content when inserting resource strings or other markup that must not be encoded.
/// </summary>
/// <param name="Value">The raw HTML string to insert as-is.</param>
/// <remarks>Use sparingly; prefer letting user-supplied values be encoded via the default handler behavior. Explicit intent — visible in code review.</remarks>
public readonly record struct RawHtml(string Value)
{
    /// <summary>Implicit conversion from string for trusted literal HTML.</summary>
    public static implicit operator RawHtml(string value) => new(value);
}
