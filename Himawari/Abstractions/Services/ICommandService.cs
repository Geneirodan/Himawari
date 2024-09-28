using System.Globalization;
using Himawari.Models;
using WTelegram;

namespace Himawari.Abstractions.Services;

public interface ICommandService
{
    CommandInfo? GetCommandByName(string command);
    HashSet<CommandInfo>? GetCommandsByCulture(CultureInfo locale);
    Task ConfigureAsync(Bot bot);
}