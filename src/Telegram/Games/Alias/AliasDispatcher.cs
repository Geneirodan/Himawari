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

/// <summary>
/// Handles Alias game: forwards text messages to <see cref="IAliasService.VerifyWord"/> and sends <see cref="GuessReply"/> when guess is Correct or Partial; handles callback queries for presenter choice, see word, next word, and end game.
/// </summary>
[PublicAPI]
public sealed class AliasDispatcher(IServiceProvider serviceProvider, IAliasService aliasService)
    : AbstractDispatcher, IUpdateHandler
{
    /// <inheritdoc />
    protected override async Task OnNewMessage(Message msg)
    {
        if (msg.Text is not { } messageText) return;
        if (aliasService.GetPresenterId(msg.Chat.Id) is null)
            return;

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

    /// <inheritdoc />
    public Task OnUpdate(Update arg) =>
        arg is { Type: UpdateType.CallbackQuery, CallbackQuery: { } query }
            ? ProcessCallbackQuery(query)
            : Task.CompletedTask;

    private const string CategoryCallbackPrefix = "Alias-Category:";

    private async Task ProcessCallbackQuery(CallbackQuery query)
    {
        IBaseRequest? request = null;
        if (query.Data?.StartsWith(CategoryCallbackPrefix, StringComparison.Ordinal) == true)
        {
            var categoryKey = query.Data[CategoryCallbackPrefix.Length..];
            if (!string.IsNullOrEmpty(categoryKey))
                request = new ChooseCategoryCallback(query, categoryKey);
        }
        else
        {
            request = query.Data.Deserialize() switch
            {
                Choose => new ChoosePresenterCallback(query),
                EndGame => new EndGameCallback(query),
                SeeWord => new SeeWordCallback(query),
                NextWord => new NextWordCallback(query),
                Correct => new CorrectGuessCallback(query),
                Skip => new NextWordCallback(query),
                Stop => new EndGameCallback(query),
                _ => null
            };
        }

        if (request is not null)
        {
            var scope = serviceProvider.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
                await scope.ServiceProvider.GetRequiredService<ISender>().Send(request).ConfigureAwait(false);
        }
    }
}