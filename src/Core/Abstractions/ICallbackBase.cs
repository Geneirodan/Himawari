using Telegram.Bot.Types;

namespace Himawari.Core.Abstractions;

public interface ICallbackBase
{
    CallbackQuery Query { get; }
}