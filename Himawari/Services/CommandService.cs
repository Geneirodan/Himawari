using System.Globalization;
using Himawari.Abstractions.Services;
using Himawari.Enums;
using Himawari.Models;
using Himawari.Resources;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Localization;

namespace Himawari.Services;

public class CommandService : ICommandService
{

    private readonly HashSet<CommandInfo> _commands = [];
    private readonly Dictionary<string, CommandInfo> _commandsByName;
    private readonly Dictionary<string, HashSet<CommandInfo>> _commandsByCulture;

    public CommandService()
    {
        foreach (var locale in SupportedLanguages.Select(x => new CultureInfo(x)))
        foreach (var command in Enum.GetValues<Command>())
        {
            var botCommand = new BotCommand
            {
                Command = CommandKeys.ResourceManager.GetString(command.ToString(), locale)
                          ?? string.Empty,
                Description = CommandDescriptions.ResourceManager.GetString(command.ToString(), locale)
                              ?? string.Empty
            };
            _commands.Add(new CommandInfo(command, botCommand, locale.Name));
        }

        _commandsByName = _commands.ToDictionary(x => x.BotCommand.Command, x => x);
        _commandsByCulture = _commands.GroupBy(x => x.Locale).ToDictionary(x => x.Key, x => x.ToHashSet());
    }

    public CommandInfo? GetCommandByName(string command) => _commandsByName.GetValueOrDefault(command);
    public HashSet<CommandInfo>? GetCommandsByCulture(CultureInfo locale) => 
        _commandsByCulture.GetValueOrDefault(locale.Name);

    public async Task ConfigureAsync(Bot bot)
    {
        if (_commandsByCulture.GetValueOrDefault(SupportedLanguages.First()) is { } @default)
            await bot.SetMyCommands(@default.Select(x => x.BotCommand));
        foreach (var (locale, commandInfos) in _commandsByCulture)
            await bot.SetMyCommands(commandInfos.Select(x => x.BotCommand), languageCode: locale);
    }
}