namespace Himawari.Telegram.Core.Pipeline;

/// <summary>
/// Configuration for the bounded channel and worker pool that process incoming Telegram updates.
/// </summary>
public sealed class ChannelPipelineOptions
{
    /// <summary>Maximum number of concurrent workers reading from the channel. Default 8.</summary>
    public int MaxConcurrency { get; set; } = 8;

    /// <summary>Maximum updates in the bounded channel before backpressure (producer waits). Default 512.</summary>
    public int ChannelCapacity { get; set; } = 512;

    /// <summary>Drop updates older than this (milliseconds) when dequeued. Default 30 seconds.</summary>
    public long StalenessMs { get; set; } = 30_000;

    /// <summary>Throttle window in milliseconds (Rx): at most one update per window is forwarded. Zero disables throttling. Default 0.</summary>
    public int ThrottleMs { get; set; }

    /// <summary>Buffer size for Rx Buffer operator (batch updates before dispatching). Zero disables buffering. Default 0.</summary>
    public int BufferCount { get; set; }

    /// <summary>When true, Rx Retry is applied so a failed handler invocation resubscribes to the stream. Default false.</summary>
    public bool UseRetry { get; set; }
}
