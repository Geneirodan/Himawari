using System.Globalization;
using Himawari.Core.Enums;
using Himawari.Core.Models;
using Himawari.Web.Resources;
using Himawari.Web.Services.Abstractions;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Web.Localization;

namespace Himawari.Web.Services;

public class CommandService : ICommandService
{
    private readonly HashSet<Command> _excludedCommands = [Command.ShutUp];
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
        _commandsByCulture.GetValueOrDefault(locale.Name)?.Where(x => !_excludedCommands.Contains(x.Type)).ToHashSet();

    public async Task ConfigureAsync(Bot bot)
    {
        if (_commandsByCulture.GetValueOrDefault(SupportedLanguages.First()) is { } @default)
            await bot.SetMyCommands(
                @default.Where(x => !_excludedCommands.Contains(x.Type)).Select(x => x.BotCommand)
            );
        foreach (var (locale, commandInfos) in _commandsByCulture)
            await bot.SetMyCommands(
                commandInfos.Where(x => !_excludedCommands.Contains(x.Type)).Select(x => x.BotCommand),
                languageCode: locale);
    }
}