using MediatR;

namespace Himawari.Telegram.Core.Abstractions.Messages;

/// <summary>
/// A Telegram callback query request with no return value.
/// </summary>
public interface ICallback : IMessage, IRequest;

/// <summary>
/// A Telegram callback query request that returns a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The response type.</typeparam>
public interface ICallback<out T> : IMessage, IRequest<T>;