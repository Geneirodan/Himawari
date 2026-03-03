using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Himawari.SillyThings.Options;

/// <summary>
/// Pre-built trigger sets for the SillyThings feature: both sticker and GIF triggers match <em>whole words</em> (word boundaries)
/// and are case-insensitive. Built once at startup from <see cref="SillyThingsOptions"/>; safe for concurrent read.
/// </summary>
/// <remarks>
/// Uses compiled regex patterns <c>\b{Regex.Escape(trigger)}\b</c> with <see cref="RegexOptions.IgnoreCase"/> so that e.g. "ss" matches "SS" but not "classic".
/// <seealso cref="DependencyInjection.AddSillyThings"/>
/// </remarks>
public sealed class SillyThingsTriggers
{
    private static readonly RegexOptions WordBoundaryOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private static readonly TimeSpan RegexMatchTimeout = TimeSpan.FromMilliseconds(500);

    /// <summary>Sticker triggers: message contains any of these as a whole word (case-insensitive).</summary>
    public FrozenSet<string> StickerTriggers { get; }

    /// <summary>GIF triggers: message contains any of these as a whole word (case-insensitive).</summary>
    public FrozenSet<string> GifTriggers { get; }

    private readonly Regex[] _stickerPatterns;
    private readonly Regex[] _gifPatterns;

    /// <summary>
    /// Builds trigger sets and compiled regex patterns from <paramref name="options"/>.
    /// </summary>
    /// <param name="options">Configured options (e.g. from config).</param>
    public SillyThingsTriggers(SillyThingsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var stickerList = options.StickerTriggers?.ToList() ?? [];
        StickerTriggers = stickerList.Count == 0
            ? FrozenSet<string>.Empty
            : stickerList.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        _stickerPatterns = stickerList.Select(t => new Regex($@"\b{Regex.Escape(t)}\b", WordBoundaryOptions, RegexMatchTimeout)).ToArray();

        var gifList = options.GifTriggers?.ToList() ?? [];
        GifTriggers = gifList.Count == 0
            ? FrozenSet<string>.Empty
            : gifList.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        _gifPatterns = gifList.Select(t => new Regex($@"\b{Regex.Escape(t)}\b", WordBoundaryOptions, RegexMatchTimeout)).ToArray();
    }

    /// <summary>Returns true if <paramref name="text"/> contains any sticker trigger as a whole word (case-insensitive).</summary>
    public bool ContainsAnyStickerTrigger(ReadOnlySpan<char> text)
    {
        if (_stickerPatterns.Length == 0) return false;
        var s = text.Trim().ToString();
        foreach (var re in _stickerPatterns)
        {
            if (re.IsMatch(s))
                return true;
        }
        return false;
    }

    /// <summary>Returns true if <paramref name="text"/> contains any GIF trigger as a whole word (case-insensitive).</summary>
    public bool ContainsAnyGifTrigger(ReadOnlySpan<char> text)
    {
        if (_gifPatterns.Length == 0) return false;
        var s = text.Trim().ToString();
        foreach (var re in _gifPatterns)
        {
            if (re.IsMatch(s))
                return true;
        }
        return false;
    }
}
