using System.Runtime.CompilerServices;
using Himawari.Telegram.Core.Abstractions.Messages;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;

namespace Himawari.VideoParser.Replies;

/// <summary>
/// Streaming reply that sends the parsed media as a group (video/photo first, then audio if any) in reply to the original message.
/// Yields each batch of messages as sent; on partial failure yields an error message and continues or stops.
/// </summary>
/// <param name="Message">The message that contained the URL.</param>
/// <param name="Files">Album media items from the parser.</param>
public sealed record ParseVideoReply(Message Message, IAlbumInputMedia[] Files) : IStreamingReply
{
    /// <inheritdoc />
    public sealed class Handler(Bot bot) : IRequestHandler<ParseVideoReply, IAsyncEnumerable<Message>>
    {
        /// <inheritdoc />
        public Task<IAsyncEnumerable<Message>> Handle(ParseVideoReply request, CancellationToken cancellationToken)
            => Task.FromResult(EnumerateAsync(request, cancellationToken));

        private async IAsyncEnumerable<Message> EnumerateAsync(
            ParseVideoReply request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var (message, inputFiles) = request;
            var replyParams = new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat.Id };
            var videoPhoto = inputFiles.Where(x => x is not InputMediaAudio).ToArray();
            long? firstSentMessageId = null;
            if (videoPhoto.Length > 0)
            {
                var (batch, errorMsg) = await SendMediaGroupSafeAsync(message.Chat.Id, videoPhoto, replyParams, cancellationToken).ConfigureAwait(false);
                if (errorMsg is not null)
                {
                    yield return errorMsg;
                    yield break;
                }

                if (batch is not null)
                {
                    firstSentMessageId = batch[0].Id;
                    foreach (var m in batch)
                        yield return m;
                }
            }

            var audioFiles = inputFiles.Where(x => x is InputMediaAudio).ToArray();
            if (audioFiles.Length == 0)
                yield break;

            var replyToMessageId = (int)(firstSentMessageId ?? message.MessageId);
            var audioReplyParams = new ReplyParameters { MessageId = replyToMessageId, ChatId = message.Chat.Id };
            var (audioBatch, audioError) = await SendMediaGroupSafeAsync(message.Chat.Id, audioFiles, audioReplyParams, cancellationToken).ConfigureAwait(false);
            if (audioError is not null)
            {
                yield return audioError;
                yield break;
            }

            if (audioBatch is not null)
            {
                foreach (var m in audioBatch)
                    yield return m;
            }
        }

        private async Task<(IReadOnlyList<Message>? Batch, Message? Error)> SendMediaGroupSafeAsync(
            long chatId,
            IAlbumInputMedia[] media,
            ReplyParameters replyParams,
            CancellationToken cancellationToken)
        {
            try
            {
                var batch = await bot.SendMediaGroup(chatId, media, replyParameters: replyParams).ConfigureAwait(false);
                return (batch, null);
            }
            catch (Exception ex)
            {
                var errorMsg = await bot.SendMessage(chatId, $"Media group failed: {ex.Message}", replyParameters: replyParams).ConfigureAwait(false);
                return (null, errorMsg);
            }
        }
    }
}
