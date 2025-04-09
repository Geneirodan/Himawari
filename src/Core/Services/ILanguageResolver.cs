using System.Globalization;

namespace Himawari.Core.Services;

public interface ILanguageResolver
{
    Task<CultureInfo> GetCurrentCulture(long chatId);
}