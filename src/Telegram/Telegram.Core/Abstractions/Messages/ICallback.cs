using MediatR;

namespace Himawari.Telegram.Core.Abstractions.Messages;

public interface ICallback : IMessage, IRequest;

public interface ICallback<out T> : IMessage, IRequest<T>;