﻿using Himawari.Application.Resources;
using Himawari.Core.Abstractions;
using Himawari.Core.Abstractions.Messages;
using Himawari.Core.Attributes;
using Himawari.Core.Extensions;
using Himawari.Core.Models;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.Application.Commands;

[BotCommand("/gift")]
public sealed record GiftCommand(Message Message, string Rest) : ICommand
{
    public sealed class Handler(Bot bot) : IRequestHandler<GiftCommand, Message>
    {
        public async Task<Message> Handle(GiftCommand request, CancellationToken cancellationToken)
        {
            var (message, rest) = request;
            var arr = rest.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length == 0)
                return await bot.SendReplyMessage(message, Messages.NotUnderstandGift).ConfigureAwait(false);

            var members = await bot.GetChatMemberList(message.Chat.Id).ConfigureAwait(false);
            var username = members.FirstOrDefault(x => x.User.Username == arr[0].TrimStart('@'))?.User.Username;
            var text = username switch
            {
                null => Messages.MemberNotFound,
                not null when arr.Length == 1 => Messages.GiftNotFound,
                _ => $"{string.Format(Messages.Gift, $"@{message.From?.Username}", $"@{username}")} {arr[1]}"
            };

            return await bot.SendReplyMessage(message, text).ConfigureAwait(false);
        }
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<GiftCommand>(aliases.CurrentValue)
    {
        public override string Description => CommandDescriptions.Gift;
        public override Func<Message, string, ICommand> Factory => (message, rest) => new GiftCommand(message, rest);
    }
}