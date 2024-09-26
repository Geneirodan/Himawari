using Himawari.Services;
using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Commands;

public record GiftCommand(Message Message, CommandInfo CommandInfo) : ICommand
{
    public class Handler : IRequestHandler<GiftCommand, Message>
    {
        public Task<Message> Handle(GiftCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}