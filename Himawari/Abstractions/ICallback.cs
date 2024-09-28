using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Abstractions;

public interface ICallback : ICallbackBase, IRequest;

public interface ICallback<out T> : ICallbackBase, IRequest<T>;

public interface ICallbackBase
{
    CallbackQuery Query { get; }
}