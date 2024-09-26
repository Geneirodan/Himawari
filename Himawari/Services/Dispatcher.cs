using Himawari.Commands;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WTelegram;

namespace Himawari.Services;

internal class Dispatcher(ILogger<Dispatcher> logger, Bot bot, ICommandService commandService, ISender sender)
    : IDispatcher
{
    public async Task OnMessage(Message msg, UpdateType update)
    {
        logger.LogInformation("Received message of type: {MessageType}", msg.Type);
        
        if (msg.Text is not { } messageText) return;
        
        if (messageText.StartsWith('/') && messageText.Length > 1)
        {
            var (command, text, forMe) = await ParseCommandAsync(messageText);

            if (!forMe) return;

            if (commandService.GetCommandByName(command) is { } commandInfo)
            {
                IRequest<Message>? commandClass = commandInfo.Type switch
                {
                    CommandType.Call => new CallAllCommand(msg, commandInfo),
                    CommandType.Lang => new LangCommand(msg, commandInfo),
                    CommandType.Who => new WhoCommand(msg, commandInfo, text),
                    CommandType.Gift => new GiftCommand(msg, commandInfo),
                    _ => null
                };
                if (commandClass is not null)
                    await sender.Send(commandClass);
            }
        }

        if (messageText.Contains("SS"))
            await sender.Send(new SecretCommand(msg));
    }

    private async Task<(string Command, string Text, bool ForMe)> ParseCommandAsync(string messageText)
    {
        var commandArray = messageText.Split(' ', 2, StringSplitOptions.TrimEntries);
        var command = commandArray[0][1..].Split('@');
        var rest = commandArray.Length > 1 ? commandArray[1] : string.Empty;

        var me = await bot.GetMe();
        return (command[0], rest, command.Length == 1 || command[1] == me.Username);
    }
}