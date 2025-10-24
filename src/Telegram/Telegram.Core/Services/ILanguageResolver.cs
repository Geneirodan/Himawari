using System.Globalization;

namespace Himawari.Telegram.Core.Services;

public interface ILanguageResolver
{
    Task<CultureInfo> GetCurrentCulture(long chatId);
}