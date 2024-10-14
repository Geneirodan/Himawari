using System.Globalization;
using System.Text.Json;
using DeepL;
using Himawari.Alias;
using Himawari.Alias.Callbacks;
using Himawari.Core.Enums;
using Himawari.Core.Extensions;
using Himawari.Core.Models;
using Himawari.Web.Commands;
using Himawari.Web.Services.Abstractions;
using MediatR;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WTelegram;
using Message = Telegram.Bot.Types.Message;
using Update = WTelegram.Types.Update;

namespace Himawari.Web.Services;

public class Dispatcher(
    ILogger<Dispatcher> logger,
    Bot bot,
    ICommandService commandService,
    IServiceProvider serviceProvider,
    IAliasService aliasService)
    : IDispatcher
{


    public async Task OnMessage(Message msg, UpdateType update)
    {
        // SetLang(msg.Chat.Id);
        logger.LogInformation("Received message of type: {MessageType}", msg.Type);

        if (msg.Text is not { } messageText) return;

        using var scope = serviceProvider.CreateScope();

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        if (messageText.StartsWith('/') && messageText.Length > 1)
        {
            var (command, rest, forMe) = await bot.ParseCommandAsync(messageText);

            if (!forMe) return;

            if (commandService.GetCommandByName(command) is { } commandInfo)
            {
                IRequest<Message>? commandClass = commandInfo.Type switch
                {
                    Command.Call => new CallAllCommand(msg),
                    Command.Lang => new LangCommand(msg, commandInfo.Locale),
                    Command.Who => new WhoCommand(msg, rest),
                    Command.Gift => new GiftCommand(msg, rest),
                    Command.Help => new HelpCommand(msg),
                    Command.Alias => new AliasCommand(msg),
                    Command.ShutUp => new ShutUpCommand(msg),
                    _ => null
                };
                if (commandClass is not null)
                    await sender.Send(commandClass);
            }
        }

        if (messageText.Contains("SS"))
            await sender.Send(new SecretMessage(msg));

        if (messageText.Equals("какіш", StringComparison.InvariantCultureIgnoreCase))
            await sender.Send(new PoopMessage(msg));

        if (messageText.Equals(await aliasService.GetCurrentWordAsync(msg.Chat.Id),
                StringComparison.InvariantCultureIgnoreCase))
            await sender.Send(new WinMessage(msg));
    }

    public Task OnUpdate(Update arg)
    {
        return arg switch
        {
            { Type: UpdateType.CallbackQuery, CallbackQuery: { } query } => OnCallbackQuery(query),
            _ => Task.CompletedTask
        };
    }

    private async Task OnCallbackQuery(CallbackQuery query)
    {
        if (query.Data is null)
            return;

        var commandInfo = JsonSerializer.Deserialize<LocalizedCallback>(query.Data);
        if (commandInfo is null)
            return;

        // if (query.Message is not null)
        //     SetLang(query.Message.Chat.Id);
        IBaseRequest? req = commandInfo.Callback switch
        {
            Callback.AliasChoose => new ChoosePresenterCallback(query),
            Callback.AliasRestart => new EndGameCallback(query),
            Callback.AliasSeeWord => new SeeWordCallback(query),
            Callback.AliasNextWord => new NextWordCallback(query),
            _ => null
        };
        if (req is not null)
            using (var scope = serviceProvider.CreateScope())
                await scope.ServiceProvider.GetRequiredService<ISender>().Send(req);
    }
}