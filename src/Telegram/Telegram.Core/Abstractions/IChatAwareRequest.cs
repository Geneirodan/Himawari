namespace Himawari.Telegram.Core.Abstractions;

/// <summary>
/// Marks a MediatR request that is scoped to a Telegram chat. Used by <see cref="Pipeline.ChatConcurrencyBehavior{TRequest,TResponse}"/> to serialize execution per chat.
/// </summary>
public interface IChatAwareRequest
{
    /// <summary>The chat ID (e.g. <see cref="Telegram.Bot.Types.Message.Chat"/>.Id).</summary>
    long ChatId { get; }
}
