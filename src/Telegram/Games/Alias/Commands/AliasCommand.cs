using Himawari.Alias.Enums;
using Himawari.Alias.Extensions;
using Himawari.Alias.Services;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Commands;

[BotCommand("/alias")]
public sealed record AliasCommand(Message Message) : ICommand
{
    public sealed class Handler(Bot bot, IAliasService service) : IRequestHandler<AliasCommand, Message>
    {
        public async Task<Message> Handle(AliasCommand request, CancellationToken cancellationToken)
        {
            var chatId = request.Message.Chat.Id;
            service.EndGame(chatId);
            return await bot.SendReplyMessage(
                message: request.Message,
                text: StartGame,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(
                        text: Want,
                        callbackData: AliasCallbackType.Choose.Serialize()
                    )
                )
            ).ConfigureAwait(false);
        }


        [PublicAPI]
        public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
            : AbstractCommandDescriptor<AliasCommand>(aliases.CurrentValue)
        {
            public override string Description => Resources.AliasCommand.Description;
            public override Func<Message, string, ICommand> Factory => (message, _) => new AliasCommand(message);
        }
    }
}