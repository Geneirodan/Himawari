using System.Globalization;
using System.Text;
using Himawari.Abstractions;
using Himawari.Abstractions.Services;
using Himawari.Extensions;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WTelegram;
using static Himawari.Resources.Messages;

namespace Himawari.Commands;

public record HelpCommand(Message Message) : ICommand
{
    public class Handler(Bot bot, ICommandService commandService) : IRequestHandler<HelpCommand, Message>
    {
        public async Task<Message> Handle(HelpCommand request, CancellationToken cancellationToken)
        {
            var commands = commandService.GetCommandsByCulture(CultureInfo.CurrentUICulture) ?? [];
            var builder = commands
                .Aggregate(
                    new StringBuilder(Help).AppendLine(),
                    (b, c) =>
                    {
                        var botCommand = c.BotCommand;
                        return b.AppendLine($"\u26a1\ufe0f `/{botCommand.Command}` - {botCommand.Description}");
                    });
            return await bot.SendReplyMessage(request.Message, builder.ToString(), ParseMode.MarkdownV2);
        }
    }
}