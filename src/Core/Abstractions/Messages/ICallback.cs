using MediatR;

namespace Himawari.Core.Abstractions.Messages;

public interface ICallback : IMessage, IRequest;

public interface ICallback<out T> : IMessage, IRequest<T>;