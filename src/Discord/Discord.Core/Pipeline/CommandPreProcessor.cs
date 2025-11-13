using Himawari.Discord.Core.Abstractions;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Himawari.Discord.Core.Pipeline;

public sealed partial class CommandPreProcessor<TRequest>(ILogger<TRequest> logger) : IRequestPreProcessor<TRequest>
    where TRequest : IDiscordCommand
{
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