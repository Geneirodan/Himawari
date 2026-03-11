using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.Telegram.Core.Pipeline;

/// <summary>
/// Envelope wrapping a Telegram message update with the timestamp it was enqueued (for staleness detection).
/// </summary>
/// <param name="Message">The Telegram message.</param>
/// <param name="Type">The update type (e.g. <see cref="UpdateType.Message"/>).</param>
/// <param name="EnqueuedTimestamp">Timestamp when the update was enqueued (<see cref="TimeProvider.GetTimestamp"/>).</param>
public readonly record struct UpdateEnvelope(Message Message, UpdateType Type, long EnqueuedTimestamp);
