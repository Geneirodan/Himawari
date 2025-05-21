using Himawari.Alias.Callbacks;
using Himawari.Alias.Extensions;
using Himawari.Alias.Replies;
using Himawari.Alias.Services;
using Himawari.Core.Abstractions;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Himawari.Alias.Enums.AliasCallbackType;
using Update = WTelegram.Types.Update;

namespace Himawari.Alias;

[PublicAPI]
public sealed class AliasDispatcher(IServiceProvider serviceProvider, IAliasService aliasService)
    : AbstractDispatcher, IUpdateHandler
{
    protected override async Task OnNewMessage(Message msg)
    {
        if (msg.Text is not { } messageText) return;

        var currentWord = aliasService.GetCurrentWord(msg.Chat.Id);
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
        IBaseRequest? request = query.Data.Deserialize() switch
        {
            Choose => new ChoosePresenterCallback(query),
            EndGame => new EndGameCallback(query),
            SeeWord => new SeeWordCallback(query),
            NextWord => new NextWordCallback(query),
            _ => null
        };
        if (request is not null)
            using (var scope = serviceProvider.CreateScope())
                await scope.ServiceProvider.GetRequiredService<ISender>().Send(request).ConfigureAwait(false);
    }
}