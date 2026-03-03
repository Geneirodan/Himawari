using WTelegram.Types;

namespace Himawari.Telegram.Core.Abstractions;

/// <summary>
/// Handles raw Telegram updates (e.g. callback queries). Implementations are registered with <see cref="BotConfigurationRegistrar"/> and subscribed to <see cref="Bot.OnUpdate"/>.
/// </summary>
public interface IUpdateHandler
{
    /// <summary>
    /// Invoked when the bot receives an update.
    /// </summary>
    /// <param name="arg">The raw Telegram update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task OnUpdate(Update arg);
}