using System.Globalization;
using Himawari.Core.Abstractions.Messages;
using Telegram.Bot.Types;

namespace Himawari.Core.Services;

public interface ICommandResolver
{
    Func<Message, string, ICommand>? GetFactoryByName(string commandName);
    string? GetCommandByAlias(string alias);
    IEnumerable<BotCommand> GetCommandsByCulture(CultureInfo cultureInfo);
}