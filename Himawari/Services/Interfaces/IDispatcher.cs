using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.Services;

public interface IDispatcher
{
    Task OnMessage(Message msg, UpdateType update);
}