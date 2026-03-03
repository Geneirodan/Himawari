using Himawari.Telegram.Application.Resources;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.RateLimiting;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Random = System.Random;

namespace Himawari.Telegram.Application.Commands;

/// <inheritdoc />
[BotCommand("/shutup")]
public sealed record ShutUpCommand(Message Message) : ICommand
{
    public sealed class Handler(IOutgoingTelegramBot bot, IOptionsMonitor<Options> optionsMonitor)
        : IRequestHandler<ShutUpCommand, Message?>
    {
        public async Task<Message?> Handle(ShutUpCommand request, CancellationToken cancellationToken)
        {
            var message = request.Message;
            var parameters = message.ReplyToMessage is { } reply
                ? new ReplyParameters
                {
                    ChatId = reply.Chat.Id,
                    MessageId = reply.MessageId
                }
                : new ReplyParameters
                {
                    ChatId = message.Chat.Id,
                    MessageId = message.MessageId
                };
            var gifUrls = optionsMonitor.CurrentValue.GifUrls;
            if (gifUrls is null || gifUrls.Length == 0)
                return null;
            var index = Random.Shared.Next(gifUrls.Length);
            return await bot.SendAnimationAsync(
                message.Chat.Id,
                gifUrls[index],
                parameters,
                cancellationToken
            ).ConfigureAwait(false);
        }
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<ShutUpCommand>(aliases.CurrentValue)
    {
        public override string Description => CommandDescriptions.ShutUp;
        public override Func<Message, string, ICommand> Factory => (message, _) => new ShutUpCommand(message);
    }

    public sealed record Options
    {
        public required string[] GifUrls { get; init; }
    }
}