using System.Globalization;
using System.Text.Json;
using Himawari.Abstractions.Services;
using Himawari.Commands;
using Himawari.Enums;
using Himawari.Extensions;
using Himawari.Games.Alias;
using Himawari.Games.Alias.Callbacks;
using Himawari.Models;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WTelegram;
using Message = Telegram.Bot.Types.Message;
using Update = WTelegram.Types.Update;

namespace Himawari.Services;

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
        logger.LogInformation("Received message of type: {MessageType}", msg.Type);

        if (msg.Text is not { } messageText) return;

        using var scope = serviceProvider.CreateScope() ;
            
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        if (messageText.StartsWith('/') && messageText.Length > 1)
        {
            var (command, rest, forMe) = await bot.ParseCommandAsync(messageText).ConfigureAwait(false);

            if (!forMe) return;

            if (commandService.GetCommandByName(command) is { } commandInfo)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(commandInfo.Locale);
                IRequest<Message>? commandClass = commandInfo.Type switch
                {
                    Command.Call => new CallAllCommand(msg),
                    Command.Lang => new LangCommand(msg),
                    Command.Who => new WhoCommand(msg, rest),
                    Command.Gift => new GiftCommand(msg, rest),
                    Command.Help => new HelpCommand(msg),
                    Command.Alias => new AliasCommand(msg),
                    _ => null
                };
                if (commandClass is not null)
                    await sender.Send(commandClass).ConfigureAwait(false);
            }
        }

        if (messageText.Contains("SS"))
            await sender.Send(new SecretMessage(msg)).ConfigureAwait(false);

        if(messageText == await aliasService.GetCurrentWordAsync(msg.Chat.Id).ConfigureAwait(false))
            await sender.Send(new WinMessage(msg)).ConfigureAwait(false);
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
        if (query.Data is not null)
        {
            var commandInfo = JsonSerializer.Deserialize<LocalizedCallback>(query.Data);
            if (commandInfo is not null)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(commandInfo.Language);
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
                        await scope.ServiceProvider.GetRequiredService<ISender>().Send(req).ConfigureAwait(false);
            }
        }
    }
}