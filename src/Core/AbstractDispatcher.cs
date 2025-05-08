using Himawari.Core.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.Core;

public abstract class AbstractDispatcher : IMessageHandler
{
    public async Task OnMessage(Message msg, UpdateType update)
    {
        if (update == UpdateType.Message)
            await OnNewMessage(msg).ConfigureAwait(false);
    }

    protected abstract Task OnNewMessage(Message msg);
}