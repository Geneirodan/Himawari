using System.Globalization;
using System.Text;
using Himawari.Telegram.Application.Resources;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Telegram.Application.Resources.Messages;

namespace Himawari.Telegram.Application.Commands;

[BotCommand("/help")]
public sealed record HelpCommand(Message Message) : ICommand
{
    public sealed class Handler(Bot bot, ICommandResolver resolver) : IRequestHandler<HelpCommand, Message>
    {
        public async Task<Message> Handle(HelpCommand request, CancellationToken cancellationToken)
        {
            var help = resolver.GetCommandsByCulture(CultureInfo.CurrentUICulture)
                .Aggregate(
                    new StringBuilder(Help).AppendLine(),
                    (b, c) => b.AppendLine($"\u26a1\ufe0f `{c.Command}` - {c.Description}"),
                    x => x.ToString()
                );

            return await bot.SendReplyMessage(request.Message, help).ConfigureAwait(false);
        }
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<HelpCommand>(aliases.CurrentValue)
    {
        public override string Description => CommandDescriptions.Help;
        public override Func<Message, string, ICommand> Factory => (message, _) => new HelpCommand(message);
    }
}