using Himawari.Discord.Core.Abstractions;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Himawari.Discord.Core.Pipeline;

/// <summary>
/// MediatR pre-processor for <see cref="IDiscordCommand"/> requests: runs before the command handler and logs the command name, full command text, member, and guild.
/// </summary>
/// <typeparam name="TRequest">The Discord command type (must implement <see cref="IDiscordCommand"/>).</typeparam>
/// <param name="logger">Logger for the request type.</param>
public sealed partial class CommandPreProcessor<TRequest>(ILogger<TRequest> logger) : IRequestPreProcessor<TRequest>
    where TRequest : IDiscordCommand
{
    /// <inheritdoc />
    /// <param name="request">The Discord command being executed (provides <see cref="IDiscordCommand.Context"/> for logging).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. Completes before the handler is invoked.</returns>
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        LogCommand(
            typeof(TRequest).Name,
            request.Context.FullCommandName,
            request.Context.Member?.ToString(),
            request.Context.Guild?.ToString()
        );
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information,
        "Executing command: {CommandName} with call \"/{Text}\" by {Member} in {Guild}")]
    private partial void LogCommand(string commandName, string? text, string? member, string? guild);
}