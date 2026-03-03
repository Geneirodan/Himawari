using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text;

namespace Himawari.Telegram.Core.Html;

/// <summary>
/// Interpolated string handler that automatically HTML-encodes all interpolated values while preserving literal HTML markup.
/// Use with <c>SendHtmlReply</c> / <c>SendHtmlMessage</c> in <c>BotExtensions</c> so that user-supplied content is safe for Telegram HTML parse mode.
/// </summary>
/// <remarks>Analogous to <c>FormattableString</c> in EF Core raw SQL or <c>IHtmlContent</c> in ASP.NET Core — makes unencoded user content impossible at the type level.</remarks>
[InterpolatedStringHandler]
public ref struct TelegramHtmlHandler
{
    private readonly StringBuilder _sb;

    /// <summary>Creates a handler with initial capacity for the interpolated string segments.</summary>
    /// <param name="literalLength">Total length of literal segments.</param>
    /// <param name="formattedCount">Number of interpolated holes.</param>
    public TelegramHtmlHandler(int literalLength, int formattedCount)
        => _sb = new StringBuilder(literalLength + formattedCount * 24);

    /// <summary>Appends literal HTML markup — trusted, not encoded.</summary>
    /// <param name="value">The literal segment (e.g. tags like <c>&lt;blockquote&gt;</c>).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLiteral(string value) => _sb.Append(value);

    /// <summary>
    /// Appends user-supplied <paramref name="value"/> — automatically HTML-encoded via <see cref="HtmlEncoder.Default"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value (typically <see cref="string"/>).</typeparam>
    /// <param name="value">The value to encode and append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted<T>(T value)
        => _sb.Append(HtmlEncoder.Default.Encode(value?.ToString() ?? string.Empty));

    /// <summary>
    /// Appends a <see cref="RawHtml"/> value without encoding — use only for pre-validated trusted content (e.g. resource strings).
    /// </summary>
    /// <param name="trusted">The trusted HTML content.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted(RawHtml trusted) => _sb.Append(trusted.Value);

    /// <summary>Returns the built HTML string.</summary>
    /// <returns>The full string with literals and encoded interpolated values.</returns>
    public readonly string Build() => _sb.ToString();
}
