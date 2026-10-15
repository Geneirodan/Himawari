using Himawari.Telegram.Core.Abstractions.Messages;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Himawari.Telegram.Core.Pipeline;

public sealed partial class CommandPreProcessor<TRequest, TResponse>(ILogger<TRequest> logger)
    : MessagePreProcessor<TRequest, TResponse>
    where TRequest : ITelegramCommand
{
    [LoggerMessage(LogLevel.Information, "Executing command: {CommandName} with call \"{Text}\"")]
    private partial void LogCommand(string commandName, string? text);

    protected override ValueTask Handle(TRequest message, CancellationToken cancellationToken)
    {
        LogCommand(typeof(TRequest).Name, message.Message?.Text);
        return ValueTask.CompletedTask;
    }
}
