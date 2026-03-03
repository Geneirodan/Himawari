using Himawari.Telegram.Core.Abstractions;
using MediatR;

namespace Himawari.Telegram.Core.Pipeline;

/// <summary>
/// MediatR behavior that serializes command execution per <see cref="IChatAwareRequest.ChatId"/>, preventing race conditions within one chat while allowing full parallelism across chats.
/// </summary>
/// <typeparam name="TRequest">The request type (must implement <see cref="IChatAwareRequest"/>).</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ChatConcurrencyBehavior<TRequest, TResponse>(
    KeyedSemaphore<long> chatLocks) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IChatAwareRequest
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        await chatLocks.WaitAsync(request.ChatId, cancellationToken).ConfigureAwait(false);
        try
        {
            return await next().ConfigureAwait(false);
        }
        finally
        {
            chatLocks.Release(request.ChatId);
        }
    }
}
