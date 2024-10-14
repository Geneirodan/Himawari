using System.Globalization;
using Himawari.Core.Models;
using WTelegram;

namespace Himawari.Web.Services.Abstractions;

public interface ICommandService
{
    CommandInfo? GetCommandByName(string command);
    HashSet<CommandInfo>? GetCommandsByCulture(CultureInfo locale);
    Task ConfigureAsync(Bot bot);
}