using Himawari.Core.Abstractions.Messages;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Himawari.Core.Pipeline;

public sealed partial class CommandPreProcessor<TRequest>(ILogger<TRequest> logger) : IRequestPreProcessor<TRequest>
    where TRequest : ICommand
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        LogCommand(typeof(TRequest).Name, request.Message?.Text);
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "Executing command: {CommandName} with call \"{Text}\"")]
    private partial void LogCommand(string commandName, string? text);
}