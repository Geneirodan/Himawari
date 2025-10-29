using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;

namespace Himawari.Discord.Music.Extensions;

public static class ResultExtensions
{
    public static Task HandleResult(
        this IResult result,
        InteractionContext context,
        Func<Task> successCallback
    ) => result.HandleResult(
        successCallback,
        async () => await context.CreateResponseWithContent(result.Errors.First(), asEphemeral: true)
            .ConfigureAwait(false)
    );

    public static Task HandleResult(
        this IResult result,
        Func<Task> successCallback,
        Func<Task> failureCallback
    ) => result.IsOk() ? successCallback() : failureCallback();
}