using WTelegram.Types;

namespace Himawari.Telegram.Core.Abstractions;

public interface IUpdateHandler
{
    Task OnUpdate(Update arg);
}