using System.Globalization;
using Himawari.Resources;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Services;

public class CommandService : ICommandService
{
    public string[] SupportedLanguages { get; } = ["en-US", "uk-UA"];

    private readonly HashSet<CommandInfo> _commands = [];
    private readonly Dictionary<string, CommandInfo> _commandsByName;
    private readonly Dictionary<CultureInfo, HashSet<CommandInfo>> _commandsByCulture;

    public CommandService()
    {
        foreach (var cultureInfo in SupportedLanguages.Select(x => new CultureInfo(x)))
        foreach (var command in Enum.GetValues<CommandType>())
        {
            var botCommand = new BotCommand
            {
                Command = CommandKeys.ResourceManager.GetString(command.ToString(), cultureInfo)
                          ?? string.Empty,
                Description = CommandDescriptions.ResourceManager.GetString(command.ToString(), cultureInfo)
                              ?? string.Empty
            };
            _commands.Add(new CommandInfo(command, botCommand, cultureInfo));
        }

        _commandsByName = _commands.ToDictionary(x => x.Command.Command, x => x);
        _commandsByCulture = _commands.GroupBy(x => x.CultureInfo).ToDictionary(x => x.Key, x => x.ToHashSet());
    }

    public CommandInfo? GetCommandByName(string command) => _commandsByName.GetValueOrDefault(command);

    public void Configure(Bot bot)
    {
        foreach (var (cultureInfo, commandInfos) in _commandsByCulture)
            bot.SetMyCommands(commandInfos.Select(x => x.Command), languageCode: cultureInfo.Name);
    }
}