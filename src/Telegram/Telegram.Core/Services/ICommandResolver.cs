using System.Globalization;
using Himawari.Telegram.Core.Abstractions.Messages;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Services;

public interface ICommandResolver
{
    Func<Message, string, ICommand>? GetFactoryByName(string commandName);
    string? GetCommandByAlias(string alias);
    IEnumerable<BotCommand> GetCommandsByCulture(CultureInfo cultureInfo);
}