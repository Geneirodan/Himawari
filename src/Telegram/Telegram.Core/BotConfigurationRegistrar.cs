using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Pipeline;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using WTelegram;

namespace Himawari.Telegram.Core;

/// <summary>
/// Fluent registrar for Telegram message and update handlers. Handlers are registered as keyed singletons and later subscribed to <see cref="Bot.OnMessage"/> and <see cref="Bot.OnUpdate"/> when <see cref="RegisterHandlers(IServiceProvider)"/> is called.
/// </summary>
[PublicAPI]
public sealed class BotConfigurationRegistrar
{
    private readonly List<string> _messageHandlers = [];
    private readonly List<Action<IServiceCollection>> _registrations = [];
    private readonly List<string> _updateHandlers = [];

    /// <summary>
    /// Registers a message handler that will be invoked for each incoming message (in registration order).
    /// </summary>
    /// <typeparam name="T">Handler type implementing <see cref="IMessageHandler"/>.</typeparam>
    /// <returns>This registrar for chaining.</returns>
    public BotConfigurationRegistrar AddMessageHandler<T>() where T : class, IMessageHandler
    {
        var serviceKey = $"{nameof(IMessageHandler)}:{typeof(T).Name}";
        _registrations.Add(x => x.AddKeyedSingleton<IMessageHandler, T>(serviceKey));
        _messageHandlers.Add(serviceKey);
        return this;
    }

    /// <summary>
    /// Registers an update handler that will be invoked for each raw update (e.g. callbacks).
    /// </summary>
    /// <typeparam name="T">Handler type implementing <see cref="IUpdateHandler"/>.</typeparam>
    /// <returns>This registrar for chaining.</returns>
    public BotConfigurationRegistrar AddUpdateHandler<T>() where T : class, IUpdateHandler
    {
        var serviceKey = $"{nameof(IUpdateHandler)}:{typeof(T).Name}";
        _registrations.Add(x => x.AddKeyedSingleton<IUpdateHandler, T>(serviceKey));
        _updateHandlers.Add(serviceKey);
        return this;
    }

    /// <summary>
    /// Subscribes all registered message and update handlers to the <see cref="Bot"/> instance. When <see cref="Pipeline.TelegramUpdateChannel"/> is registered, message updates are enqueued to the channel instead of invoking handlers directly (worker pool processes them).
    /// </summary>
    /// <param name="serviceProvider">The application service provider (must resolve <see cref="Bot"/> and keyed handlers).</param>
    public void RegisterHandlers(IServiceProvider serviceProvider)
    {
        var bot = serviceProvider.GetRequiredService<Bot>();

        var channel = serviceProvider.GetService<TelegramUpdateChannel>();
        if (channel is not null)
        {
            var time = serviceProvider.GetRequiredService<TimeProvider>();
            bot.OnMessage += async (msg, type) =>
            {
                await channel.Writer.WriteAsync(
                    new UpdateEnvelope(msg, type, time.GetTimestamp())).ConfigureAwait(false);
            };
        }
        else
        {
            foreach (var handler in _messageHandlers.Select(serviceProvider.GetRequiredKeyedService<IMessageHandler>))
                bot.OnMessage += handler.OnMessage;
        }

        foreach (var handler in _updateHandlers.Select(serviceProvider.GetRequiredKeyedService<IUpdateHandler>))
            bot.OnUpdate += handler.OnUpdate;
    }

    /// <summary>
    /// Ordered list of message handler service keys (for keyed resolution in <see cref="TelegramUpdateWorkerService"/>).
    /// </summary>
    internal IReadOnlyList<string> MessageHandlerKeys => _messageHandlers;

    /// <summary>
    /// Applies all handler registrations to the service collection and registers this registrar as a singleton.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    public void RegisterHandlers(IServiceCollection serviceCollection)
    {
        _registrations.ForEach(x => x.Invoke(serviceCollection));
        serviceCollection.AddSingleton(this);
    }
}