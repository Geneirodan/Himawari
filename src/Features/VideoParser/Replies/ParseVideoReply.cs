using Himawari.Core.Abstractions.Messages;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.VideoParser.Replies;

public record ParseVideoReply(Message Message, IAlbumInputMedia[] Files) : IReply
{
    public class Handler(Bot bot) : IRequestHandler<ParseVideoReply, IEnumerable<Message>>
    {
        public async Task<IEnumerable<Message>> Handle(ParseVideoReply request, CancellationToken cancellationToken)
        {
            var (message, inputFiles) = request;
            return await bot.SendMediaGroup(
                chatId: message.Chat.Id,
                media: inputFiles,
                replyParameters: new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat.Id }
            ).ConfigureAwait(false);
        }
    }
}