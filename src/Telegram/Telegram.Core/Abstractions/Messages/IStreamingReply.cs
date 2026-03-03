using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions.Messages;

/// <summary>
/// A Telegram message request that returns a stream of <see cref="Message"/> instances (e.g. for galleries or partial results with per-item errors).
/// </summary>
/// <remarks>
/// Use <see cref="IStreamingReply"/> when the handler yields messages as they are produced (e.g. one batch at a time) or reports partial failures.
/// The caller must consume the returned <see cref="IAsyncEnumerable{T}"/> (e.g. with <c>await foreach</c>) to drive the handler to completion.
/// </remarks>
public interface IStreamingReply : IMessage, IRequest<IAsyncEnumerable<Message>>;
