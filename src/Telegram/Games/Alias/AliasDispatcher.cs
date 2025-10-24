using Himawari.Alias.Callbacks;
using Himawari.Alias.Enums;
using Himawari.Alias.Extensions;
using Himawari.Alias.Replies;
using Himawari.Alias.Services;
using Himawari.Telegram.Core.Abstractions;
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

        var response = aliasService.VerifyWord(msg.Chat.Id, messageText) switch
        {
            Guess.Partial => new GuessReply(msg, IsCorrect: false),
            Guess.Correct => new GuessReply(msg, IsCorrect: true),
            _ => null
        };
        
        if (response is not null)
        {
            var scope = serviceProvider.CreateAsyncScope();
            await using (scope.ConfigureAwait(false)) 
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
        {
            var scope = serviceProvider.CreateAsyncScope();
            await using (scope.ConfigureAwait(false)) 
                await scope.ServiceProvider.GetRequiredService<ISender>().Send(request).ConfigureAwait(false);
        }
    }
}