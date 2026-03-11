using System.Collections.Frozen;
using System.Globalization;
using Microsoft.Extensions.Options;

namespace Himawari.Telegram.Core.Localization;

/// <summary>
/// Resolves the best-match supported bot culture from a Telegram BCP-47 LanguageCode.
/// Supported output cultures: en (en), ru-RU, uk-UA. Uses a proper "en" culture so resource fallback (.en.resx) and TwoLetterISOLanguageName work.
/// Fallback policy: only "ru" → ru-RU and "uk" → uk-UA by default; post-Soviet and Slavic EU languages
/// map to EN for political neutrality. The mapping is overridable via <see cref="LocalizationOptions.Mappings"/> in appsettings.json.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>"ru" → ru-RU, "uk" → uk-UA, "en" → en (direct support).</description></item>
/// <item><description>Belarusian (be): EN by default (politically sensitive); config may set "be" → "uk-UA".</description></item>
/// <item><description>Baltic (et, lv, lt), Caucasus (ka, hy, az), Central Asia (kk, ky, uz, tg, tk): EN.</description></item>
/// <item><description>Slavic EU (pl, cs, sk, bg, sr, hr, bs, mk, sl), Romanian (ro, mo): EN.</description></item>
/// <item><description>Any other tag → EN (safe neutral default).</description></item>
/// </list>
/// </remarks>
public sealed class LanguageFallbackPolicy : ICultureResolver
{
    private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru-RU");
    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");
    /// <summary>English culture (en) for resource fallback and TwoLetterISOLanguageName; not InvariantCulture.</summary>
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");

    private static readonly FrozenDictionary<string, CultureInfo> BuiltIn = new Dictionary<string, CultureInfo>(StringComparer.OrdinalIgnoreCase)
    {
        ["ru"] = Russian,
        ["uk"] = Ukrainian,
        ["en"] = English,
        ["et"] = English,
        ["lv"] = English,
        ["lt"] = English,
        ["ka"] = English,
        ["hy"] = English,
        ["az"] = English,
        ["kk"] = English,
        ["ky"] = English,
        ["uz"] = English,
        ["tg"] = English,
        ["tk"] = English,
        ["be"] = English,
        ["ro"] = English,
        ["mo"] = English,
        ["pl"] = English,
        ["cs"] = English,
        ["sk"] = English,
        ["bg"] = English,
        ["sr"] = English,
        ["hr"] = English,
        ["bs"] = English,
        ["mk"] = English,
        ["sl"] = English
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private readonly FrozenDictionary<string, CultureInfo> _effective;

    /// <summary>
    /// Creates the policy with optional overrides from <paramref name="options"/>.
    /// Config overrides win over built-in mappings.
    /// </summary>
    public LanguageFallbackPolicy(IOptions<LocalizationOptions> options)
    {
        var merged = new Dictionary<string, CultureInfo>(BuiltIn, StringComparer.OrdinalIgnoreCase);

        foreach (var (tag, cultureName) in options.Value.Mappings)
        {
            try
            {
                merged[tag] = string.IsNullOrEmpty(cultureName)
                    ? English
                    : CultureInfo.GetCultureInfo(cultureName);
            }
            catch (CultureNotFoundException)
            {
                // Ignore invalid culture names from config
            }
        }

        _effective = merged.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public CultureInfo Resolve(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return English;

        var primary = languageCode.Contains('-', StringComparison.Ordinal)
            ? languageCode[..languageCode.IndexOf('-', StringComparison.Ordinal)]
            : languageCode;

        return _effective.TryGetValue(primary, out var culture) ? culture : English;
    }
}
