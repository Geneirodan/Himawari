using System.Collections;
using System.Globalization;
using System.Text;
using Himawari.Application.Resources;
using Himawari.Core.Abstractions;
using Himawari.Core.Abstractions.Messages;
using Himawari.Core.Attributes;
using Himawari.Core.Extensions;
using Himawari.Core.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WTelegram;

namespace Himawari.Application.Commands;

[BotCommand("/who")]
public sealed record WhoCommand(Message Message, string Rest) : ICommand
{
    public sealed class Handler(Bot bot) : IRequestHandler<WhoCommand, Message>
    {
        public async Task<Message> Handle(WhoCommand request, CancellationToken cancellationToken)
        {
            var (message, rest) = request;

            var chatMembers = await bot.GetChatMemberList(message.Chat.Id).ConfigureAwait(false);
            var members = chatMembers.Where(x => !x.User.IsBot).ToArray();

            var index = Random.Shared.Next(members.Length);

            var cultureInfo = CultureInfo.CurrentUICulture;
            var resourceSet = WhoCommandVariants.ResourceManager
                .GetResourceSet(cultureInfo, true, true)!
                .Cast<DictionaryEntry>()
                .Select(entry => entry.Key)
                .Cast<string>()
                .ToArray();

            var quote = Random.Shared.Next(resourceSet.Length);

            var text = WhoCommandVariants.ResourceManager.GetString(resourceSet[quote], cultureInfo);
            var stringBuilder = new StringBuilder(text)
                .Append(' ')
                .Append(members[index].User.GetUsername());

            if (!string.IsNullOrWhiteSpace(rest))
                stringBuilder.Append(' ').Append(rest.TrimEnd('?'));

            return await bot.SendReplyMessage(message, stringBuilder.ToString(), ParseMode.MarkdownV2)
                .ConfigureAwait(false);
        }
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<WhoCommand>(aliases.CurrentValue)
    {
        public override string Description => CommandDescriptions.Who;
        public override Func<Message, string, ICommand> Factory => (message, text) => new WhoCommand(message, text);
    }
}