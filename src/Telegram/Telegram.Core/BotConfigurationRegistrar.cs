using Himawari.Telegram.Core.Abstractions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using WTelegram;

namespace Himawari.Telegram.Core;

[PublicAPI]
public sealed class BotConfigurationRegistrar
{
    private readonly List<string> _messageHandlers = [];
    private readonly List<Action<IServiceCollection>> _registrations = [];
    private readonly List<string> _updateHandlers = [];

    public BotConfigurationRegistrar AddMessageHandler<T>() where T : class, IMessageHandler
    {
        var serviceKey = $"{nameof(IMessageHandler)}:{typeof(T).Name}";
        _registrations.Add(x => x.AddKeyedSingleton<IMessageHandler, T>(serviceKey));
        _messageHandlers.Add(serviceKey);
        return this;
    }

    public BotConfigurationRegistrar AddUpdateHandler<T>() where T : class, IUpdateHandler
    {
        var serviceKey = $"{nameof(IUpdateHandler)}:{typeof(T).Name}";
        _registrations.Add(x => x.AddKeyedSingleton<IUpdateHandler, T>(serviceKey));
        _updateHandlers.Add(serviceKey);
        return this;
    }

    public void RegisterHandlers(IServiceProvider serviceProvider)
    {
        var bot = serviceProvider.GetRequiredService<Bot>();

        foreach (var handler in _messageHandlers.Select(serviceProvider.GetRequiredKeyedService<IMessageHandler>))
            bot.OnMessage += handler.OnMessage;
        foreach (var handler in _updateHandlers.Select(serviceProvider.GetRequiredKeyedService<IUpdateHandler>))
            bot.OnUpdate += handler.OnUpdate;
    }

    public void RegisterHandlers(IServiceCollection serviceCollection)
    {
        _registrations.ForEach(x => x.Invoke(serviceCollection));
        serviceCollection.AddSingleton(this);
    }
}