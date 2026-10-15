using Mediator;

namespace Himawari.Telegram.Core.Abstractions.Messages;

public interface ICallback : ITelegramMessage, IRequest;

public interface ICallback<out T> : ITelegramMessage, IRequest<T>;
