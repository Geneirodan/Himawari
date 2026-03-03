namespace Himawari.Telegram.Core.Localization;

/// <summary>
/// Provides localized bot messages.
/// Abstraction over <c>IStringLocalizer</c> — Core has no dependency on Microsoft.Extensions.Localization.
/// </summary>
public interface IBotLocalizer
{
    /// <summary>Returns localized string by key using current <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>.</summary>
    /// <param name="key">Resource key (e.g. from <see cref="Loc"/>).</param>
    string this[string key] { get; }

    /// <summary>Returns localized string with format arguments (composite formatting).</summary>
    /// <param name="key">Resource key.</param>
    /// <param name="args">Format arguments for <c>string.Format</c>.</param>
    string this[string key, params object[] args] { get; }
}
