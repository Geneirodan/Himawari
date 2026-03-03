using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.Telegram.Core.Abstractions;

/// <summary>
/// Base implementation of <see cref="IMessageHandler"/> that forwards only <see cref="UpdateType.Message"/> to <see cref="OnNewMessage"/>.
/// </summary>
public abstract class AbstractDispatcher : IMessageHandler
{
    /// <inheritdoc />
    public async Task OnMessage(Message msg, UpdateType update)
    {
        if (update == UpdateType.Message)
            await OnNewMessage(msg).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles a new message update. Override in derived dispatchers (e.g. command routing, feature handling).
    /// </summary>
    /// <param name="msg">The Telegram message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected abstract Task OnNewMessage(Message msg);
}