using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Himawari.Telegram.Core.Pipeline;

/// <summary>
/// Bounded producer/consumer channel for incoming Telegram updates. Provides backpressure when capacity is reached and processes updates with a fixed worker pool.
/// </summary>
public sealed class TelegramUpdateChannel : IDisposable
{
    private readonly Channel<UpdateEnvelope> _channel;

    /// <summary>
    /// Creates a bounded channel with the configured capacity and <see cref="BoundedChannelFullMode.Wait"/> so producers are throttled when full.
    /// </summary>
    /// <param name="options">Pipeline options (channel capacity).</param>
    public TelegramUpdateChannel(IOptions<ChannelPipelineOptions> options)
    {
        var capacity = options.Value.ChannelCapacity;
        _channel = Channel.CreateBounded<UpdateEnvelope>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }

    /// <summary>Writer for enqueueing updates.</summary>
    public ChannelWriter<UpdateEnvelope> Writer => _channel.Writer;

    /// <summary>Reader for worker consumption.</summary>
    public ChannelReader<UpdateEnvelope> Reader => _channel.Reader;

    /// <summary>Signals that no more updates will be written.</summary>
    public void Complete() => _channel.Writer.Complete();

    /// <inheritdoc />
    public void Dispose() => Complete();
}
