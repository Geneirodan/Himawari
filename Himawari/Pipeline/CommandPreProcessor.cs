using Himawari.Commands;
using MediatR.Pipeline;

namespace Himawari.Pipeline;

public sealed class CommandPreProcessor<TRequest>(ILogger<TRequest> logger)
    : IRequestPreProcessor<TRequest>
    where TRequest : ICommand
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing command: {Name} with call \"{Text}\"", typeof(TRequest).Name,
            request.Message.Text);
        return Task.CompletedTask;
    }
}