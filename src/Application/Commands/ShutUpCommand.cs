using Himawari.Core.Abstractions;
using Himawari.Core.Attributes;
using Himawari.Core.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;
using Random = System.Random;

namespace Himawari.Application.Commands;

[BotCommand("/shutup")]
public sealed record ShutUpCommand(Message Message) : ICommand
{
    public sealed class Handler(Bot bot, IOptionsMonitor<Options> optionsMonitor) : IRequestHandler<ShutUpCommand, Message?>
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
            var index = Random.Shared.Next(gifUrls.Length);
            return await bot.SendAnimation(
                chatId: message.Chat.Id,
                animation: gifUrls[index],
                replyParameters: parameters).ConfigureAwait(false);
        }
    }
    
    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<ShutUpCommand>(aliases.CurrentValue)
    {
        public override string Description => Resources.CommandDescriptions.ShutUp;
        public override Func<Message, string, ICommand> Factory => (message, _) => new ShutUpCommand(message);
    }
    
    public sealed record Options
    {
        public required string[] GifUrls { get; init; }
    }
}