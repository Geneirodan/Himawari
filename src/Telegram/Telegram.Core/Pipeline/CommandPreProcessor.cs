using Himawari.Telegram.Core.Abstractions.Messages;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Himawari.Telegram.Core.Pipeline;

/// <summary>
/// MediatR pre-processor for <see cref="ICommand"/> requests; logs the command name and message text.
/// </summary>
/// <typeparam name="TRequest">The command type.</typeparam>
/// <param name="logger">Logger for the request type.</param>
public sealed partial class CommandPreProcessor<TRequest>(ILogger<TRequest> logger) : IRequestPreProcessor<TRequest>
    where TRequest : ICommand
{
    /// <inheritdoc />
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        LogCommand(typeof(TRequest).Name, request.Message?.Text);
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "Executing command: {CommandName} with call \"{Text}\"")]
    private partial void LogCommand(string commandName, string? text);
}