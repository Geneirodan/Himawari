using Himawari.Discord.Core.Abstractions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Himawari.Discord.Core.Pipeline;

public sealed partial class CommandPreProcessor<TRequest, TResponse>(ILogger<TRequest> logger)
    : MessagePreProcessor<TRequest, TResponse>
    where TRequest : IDiscordCommand, IBaseCommand
{
    protected override ValueTask Handle(TRequest message, CancellationToken cancellationToken)
    {
        LogCommand(
            typeof(TRequest).Name,
            message.Context.FullCommandName,
            message.Context.Member?.ToString(),
            message.Context.Guild?.ToString()
        );
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(
        LogLevel.Information,
        "Executing command: {CommandName} with call \"/{Text}\" by {Member} in {Guild}"
    )]
    private partial void LogCommand(string commandName, string? text, string? member, string? guild);
}
