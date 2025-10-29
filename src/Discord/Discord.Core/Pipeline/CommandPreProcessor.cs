using Himawari.Discord.Core.Abstractions;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Himawari.Discord.Core.Pipeline;

public sealed partial class CommandPreProcessor<TRequest>(ILogger<TRequest> logger) : IRequestPreProcessor<TRequest>
    where TRequest : IDiscordCommand
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        using (LogContext.PushProperty("Request", request, destructureObjects: true))
            LogCommand(typeof(TRequest).Name, request.Context.FullCommandName);
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "Executing command: {CommandName} with call \"{Text}\"")]
    private partial void LogCommand(string commandName, string? text);
}