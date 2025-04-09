using Himawari.Core.Abstractions;
using Himawari.Core.Extensions;
using Himawari.Core.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types.Enums;
using WTelegram;
using Message = Telegram.Bot.Types.Message;

namespace Himawari.Application;

[PublicAPI]
public sealed class CommandDispatcher(Bot bot, ICommandResolver resolver, IServiceProvider serviceProvider) 
    : IMessageHandler
{
    public async Task OnMessage(Message msg, UpdateType update)
    {
        if (msg.Text is not { } messageText) return;

        using var scope = serviceProvider.CreateScope();

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        if (messageText.StartsWith('/') && messageText.Length > 1)
        {
            var (command, rest, forMe) = await bot.ParseCommandAsync(messageText).ConfigureAwait(false);

            if (!forMe) return;

            command = resolver.GetCommandByAlias(command);
            if (command is null)
                return;

            var req = resolver.GetFactoryByName(command)?.Invoke(msg, rest);
            if (req is not null)
                await sender.Send(req).ConfigureAwait(false);
        }
    }
}