using Himawari.Telegram.Core.Abstractions.Messages;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.VideoParser.Replies;

public sealed record ParseVideoReply(Message Message, IAlbumInputMedia[] Files) : IReply
{
    public sealed class Handler(Bot bot) : IRequestHandler<ParseVideoReply, IEnumerable<Message>>
    {
        public async Task<IEnumerable<Message>> Handle(ParseVideoReply request, CancellationToken cancellationToken)
        {
            var (message, inputFiles) = request;
            List<Message> messages =
            [
                ..await bot.SendMediaGroup(
                    chatId: message.Chat.Id,
                    media: inputFiles.Where(x => x is not InputMediaAudio),
                    replyParameters: new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat.Id }
                ).ConfigureAwait(false)
            ];
            var audioFiles = inputFiles.Where(x => x is InputMediaAudio).ToArray();
            if (audioFiles.Length > 0)
                messages.AddRange(
                    await bot.SendMediaGroup(
                        chatId: message.Chat.Id,
                        media: audioFiles,
                        replyParameters: new ReplyParameters { MessageId = messages[0].Id, ChatId = message.Chat.Id }
                    ).ConfigureAwait(false)
                );
            return messages;
        }
    }
}