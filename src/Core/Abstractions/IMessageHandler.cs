using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.Core.Abstractions;

public interface IMessageHandler
{
    Task OnMessage(Message msg, UpdateType update);
}