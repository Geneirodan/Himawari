using Ardalis.Result;
using DisCatSharp.ApplicationCommands.Context;

namespace Himawari.Discord.Music.Extensions;

/// <summary>Extension methods to run success or failure callbacks based on <see cref="IResult"/> (used by slash command handlers to edit or create the response).</summary>
public static class ResultExtensions
{
    /// <summary>Runs <paramref name="successCallback"/> when the result is success; otherwise creates an ephemeral response with the first error.</summary>
    public static Task HandleResult(
        this IResult result,
        InteractionContext context,
        Func<Task> successCallback
    ) => result.HandleResult(
        successCallback,
        async () => await context.CreateResponseWithContent(result.Errors.First(), asEphemeral: true)
            .ConfigureAwait(false)
    );

    /// <summary>Runs <paramref name="successCallback"/> or <paramref name="failureCallback"/> depending on <see cref="IResult.IsOk"/>.</summary>
    public static Task HandleResult(
        this IResult result,
        Func<Task> successCallback,
        Func<Task> failureCallback
    ) => result.IsOk() ? successCallback() : failureCallback();
}