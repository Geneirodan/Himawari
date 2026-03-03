using System.Globalization;
using System.Text.Encodings.Web;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Html;
using Himawari.Telegram.Core.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Himawari.Telegram.Core.RateLimiting;
using Telegram.Bot.Types;
using static System.StringComparison;

namespace Himawari.Todolist;

/// <inheritdoc />
[BotCommand("/todo")]
public sealed record TodoCommand(Message Message) : ICommand, IChatAwareRequest
{
    /// <inheritdoc />
    public long ChatId => Message.Chat.Id;

    /// <summary>Handles /todo: forwards the message text to the configured admin and replies to the user.</summary>
    public sealed class Handler(IOutgoingTelegramBot bot, IOptions<Options> options) : IRequestHandler<TodoCommand, Message>
    {
        public async Task<Message> Handle(TodoCommand request, CancellationToken cancellationToken)
        {
            var advice = request.Message.Text is { Length: > 0 } t
                ? t[(t.IndexOf(' ', OrdinalIgnoreCase) + 1)..]
                : string.Empty;
            var adminId = options.Value.AdminId;
            if (adminId != 0)
            {
                var senderEncoded = HtmlEncoder.Default.Encode(
                    request.Message.From?.GetDisplayName() ?? string.Empty);
                await bot.SendHtmlMessage(
                    adminId,
                    $"{(RawHtml)string.Format(CultureInfo.CurrentUICulture, Resources.Text, senderEncoded)}\n<blockquote>{advice}</blockquote>",
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }
            return await bot.SendReplyMessage(request.Message, Resources.Sent, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<TodoCommand>(aliases.CurrentValue)
    {
        public override string Description => Resources.CommandDescription;
        public override Func<Message, string, ICommand> Factory => (message, _) => new TodoCommand(message);
    }

    /// <summary>Configuration for the todo command (e.g. admin chat ID to forward to).</summary>
    public record Options
    {
        /// <summary>Telegram chat or user ID that receives the forwarded todo message.</summary>
        public int AdminId { get; init; }
    }
}