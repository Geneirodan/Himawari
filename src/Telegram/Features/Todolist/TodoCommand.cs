using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WTelegram;

namespace Himawari.Todolist;

[BotCommand("/todo")]
public sealed record TodoCommand(Message Message) : ICommand
{
    public sealed class Handler(Bot bot, IOptions<Options> options) : IRequestHandler<TodoCommand, Message>
    {
        public async Task<Message> Handle(TodoCommand request, CancellationToken cancellationToken)
        {
            var sender = request.Message.From?.GetDisplayName();
            var text = $"{string.Format(Resources.Text, sender)}\n`{request.Message.Text?["/todo ".Length..]}`";
            await bot.SendMessage(
                chatId: options.Value.AdminId,
                text: text,
                parseMode: ParseMode.MarkdownV2
            ).ConfigureAwait(false);
            return await bot.SendReplyMessage(request.Message, Resources.Sent).ConfigureAwait(false);
        }
    }
    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<TodoCommand>(aliases.CurrentValue)
    {
        public override string Description => Resources.CommandDescription;
        public override Func<Message, string, ICommand> Factory => (message, _) => new TodoCommand(message);
    }
    public record Options
    {
        public int AdminId { get; init; }
    }
}