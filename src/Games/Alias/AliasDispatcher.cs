using Himawari.Alias.Callbacks;
using Himawari.Alias.Models;
using Himawari.Alias.Replies;
using Himawari.Alias.Services;
using Himawari.Core.Abstractions;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Update = WTelegram.Types.Update;

namespace Himawari.Alias;

[PublicAPI]
public sealed class AliasDispatcher(IServiceProvider serviceProvider, IAliasService aliasService)
    : AbstractDispatcher, IUpdateHandler
{
    protected override async Task OnNewMessage(Message msg)
    {
        if (msg.Text is not { } messageText) return;

        // TODO: Optimize unnecessary call to alias word generator
        var currentWord = await aliasService.GetCurrentWordAsync(msg.Chat.Id).ConfigureAwait(false);
        if (messageText.Equals(currentWord, StringComparison.InvariantCultureIgnoreCase))
        {
            var response = new WinReply(msg);
            using var scope = serviceProvider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ISender>().Send(response).ConfigureAwait(false);
        }
    }

    public Task OnUpdate(Update arg) => 
        arg is { Type: UpdateType.CallbackQuery, CallbackQuery: { } query } 
            ? ProcessCallbackQuery(query) 
            : Task.CompletedTask;

    private async Task ProcessCallbackQuery(CallbackQuery query)
    {
        if (query.Data is null)
            return;

        var commandInfo = AliasCallbackData.Deserialize(query.Data);
        if (commandInfo is null)
            return;

        IBaseRequest? request = commandInfo.Callback switch
        {
            AliasCallbackData.CallbackType.Choose => new ChoosePresenterCallback(query),
            AliasCallbackData.CallbackType.Restart => new EndGameCallback(query),
            AliasCallbackData.CallbackType.SeeWord => new SeeWordCallback(query),
            AliasCallbackData.CallbackType.NextWord => new NextWordCallback(query),
            _ => null
        };
        if (request is not null)
            using (var scope = serviceProvider.CreateScope())
                await scope.ServiceProvider.GetRequiredService<ISender>().Send(request).ConfigureAwait(false);
    }
}