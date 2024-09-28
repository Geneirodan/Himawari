using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Update = WTelegram.Types.Update;

namespace Himawari.Abstractions.Services;

public interface IDispatcher
{
    Task OnMessage(Message msg, UpdateType update);
    Task OnUpdate(Update arg);
}