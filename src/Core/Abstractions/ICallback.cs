using MediatR;

namespace Himawari.Core.Abstractions;

public interface ICallback : ICallbackBase, IRequest;

public interface ICallback<out T> : ICallbackBase, IRequest<T>;