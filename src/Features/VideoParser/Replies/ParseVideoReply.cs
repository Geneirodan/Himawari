using Himawari.Core.Abstractions.Messages;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.VideoParser.Replies;

public record ParseVideoReply(Message Message, InputFile File) : IReply
{
    public class Handler(Bot bot) : IRequestHandler<ParseVideoReply, Message>
    {
        public async Task<Message> Handle(ParseVideoReply request, CancellationToken cancellationToken)
        {
            var (message, inputFile) = request;
            return await bot.SendVideo(
                    chatId: message.Chat.Id,
                    video: inputFile,
                    replyParameters: new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat.Id }
                )
                .ConfigureAwait(false);
        }
    }
}