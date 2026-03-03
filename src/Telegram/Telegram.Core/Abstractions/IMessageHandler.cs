using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.Telegram.Core.Abstractions;

/// <summary>
/// Handles incoming Telegram messages. Implementations are registered with <see cref="BotConfigurationRegistrar"/> and subscribed to <see cref="Bot.OnMessage"/>.
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// Invoked when the bot receives a message or compatible update.
    /// </summary>
    /// <param name="msg">The Telegram message.</param>
    /// <param name="update">The update type (e.g. <see cref="UpdateType.Message"/>).</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task OnMessage(Message msg, UpdateType update);
}