using System.Reactive.Linq;
using System.Reactive.Subjects;
using Himawari.Telegram.Core.Abstractions;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Himawari.Telegram.Core.Pipeline;

/// <summary>
/// Background service that consumes <see cref="TelegramUpdateChannel"/> and invokes all registered message handlers per update (resolved by key from <see cref="BotConfigurationRegistrar"/>).
/// Handlers are resolved from a new scope per envelope to avoid captive dependency when any handler or its transitive dependencies are scoped.
/// When <see cref="ChannelPipelineOptions.ThrottleMs"/>, <see cref="ChannelPipelineOptions.BufferCount"/>, or <see cref="ChannelPipelineOptions.UseRetry"/> are set, uses an Rx pipeline (Throttle, Buffer, Retry) for declarative dispatch.
/// Drops updates older than <see cref="ChannelPipelineOptions.StalenessMs"/>.
/// </summary>
public sealed class TelegramUpdateWorkerService(
    TelegramUpdateChannel channel,
    IServiceScopeFactory scopeFactory,
    TimeProvider time,
    BotConfigurationRegistrar registrar,
    ILogger<TelegramUpdateWorkerService> logger,
    IOptions<ChannelPipelineOptions> options) : BackgroundService
{
    private ChannelPipelineOptions Options => options.Value;
    private int WorkerCount => Options.MaxConcurrency;
    private long StalenessMs => Options.StalenessMs;

    private static bool UseRxPipeline(ChannelPipelineOptions opts)
        => opts.ThrottleMs > 0 || opts.BufferCount > 0 || opts.UseRetry;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (UseRxPipeline(Options))
            await RunReactivePipelineAsync(stoppingToken).ConfigureAwait(false);
        else
            await RunWorkerPoolAsync(stoppingToken).ConfigureAwait(false);
    }

    /// <summary>Classic pool: N workers read from channel and invoke handlers (no Rx).</summary>
    private async Task RunWorkerPoolAsync(CancellationToken stoppingToken)
    {
        var workers = Enumerable.Range(0, WorkerCount)
            .Select(id => RunWorkerAsync(id, stoppingToken));
        await Task.WhenAll(workers).ConfigureAwait(false);
    }

    private async Task RunWorkerAsync(int workerId, CancellationToken ct)
    {
#pragma warning disable MA0004 // ConfigureAwait not available for await foreach
        await foreach (var envelope in channel.Reader.ReadAllAsync(ct))
#pragma warning restore MA0004
        {
            if (IsStale(envelope))
            {
                logger.LogWarning("Worker {WorkerId}: dropped stale update ({Elapsed:F0}ms)", workerId, GetElapsedMs(envelope));
                continue;
            }

            try
            {
                await ProcessEnvelopeAsync(envelope).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Worker {WorkerId}: unhandled error processing update", workerId);
            }
        }
    }

    /// <summary>Single reader feeds Subject (thread-safe: only one writer); Rx pipeline (Throttle / Buffer / Retry) then Subscribe with bounded concurrency.</summary>
    private async Task RunReactivePipelineAsync(CancellationToken stoppingToken)
    {
        var subject = new Subject<UpdateEnvelope>();
        var opts = Options;
        var semaphore = new SemaphoreSlim(opts.MaxConcurrency, opts.MaxConcurrency);

        IObservable<UpdateEnvelope> stream = subject
            .Where(e => !IsStale(e));
        if (opts.ThrottleMs > 0)
            stream = stream.Throttle(TimeSpan.FromMilliseconds(opts.ThrottleMs));
        if (opts.BufferCount > 0)
        {
            var buffered = stream.Buffer(opts.BufferCount);
            stream = buffered.SelectMany(batch => batch.ToObservable());
        }

        IObservable<UpdateEnvelope> pipeline = opts.UseRetry ? stream.Retry() : stream;

        var subscription = pipeline.Subscribe(
            envelope => _ = ProcessWithSemaphoreAsync(envelope, semaphore),
            ex => logger.LogError(ex, "Reactive pipeline error"),
            () => { });

        var readerTask = FeedSubjectAsync(subject, stoppingToken);
        try
        {
            await readerTask.ConfigureAwait(false);
        }
        finally
        {
            subscription.Dispose();
            semaphore.Dispose();
        }
    }

    /// <summary>Single-threaded reader: reads from channel and pushes to Subject. Subject is not thread-safe for concurrent OnNext; this is the only writer.</summary>
    private async Task FeedSubjectAsync(Subject<UpdateEnvelope> subject, CancellationToken ct)
    {
        try
        {
#pragma warning disable MA0004
            await foreach (var envelope in channel.Reader.ReadAllAsync(ct))
#pragma warning restore MA0004
                subject.OnNext(envelope);
            subject.OnCompleted();
        }
        catch (OperationCanceledException)
        {
            subject.OnCompleted();
        }
        catch (Exception ex)
        {
            subject.OnError(ex);
        }
    }

    private async Task ProcessWithSemaphoreAsync(UpdateEnvelope envelope, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            await ProcessEnvelopeAsync(envelope).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }

    [SuppressMessage("Reliability", "MA0004:Use Task.ConfigureAwait(false)", Justification = "CreateAsyncScope returns AsyncServiceScope; .ConfigureAwait(false) yields ConfiguredAsyncDisposable which has no ServiceProvider.")]
    private async Task ProcessEnvelopeAsync(UpdateEnvelope envelope)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        try
        {
            foreach (var key in registrar.MessageHandlerKeys)
            {
                var handler = scope.ServiceProvider.GetRequiredKeyedService<IMessageHandler>(key);
                await handler.OnMessage(envelope.Message, envelope.Type).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error processing update");
            throw;
        }
    }

    private bool IsStale(UpdateEnvelope envelope)
        => GetElapsedMs(envelope) > StalenessMs;

    private double GetElapsedMs(UpdateEnvelope envelope)
        => time.GetElapsedTime(envelope.EnqueuedTimestamp).TotalMilliseconds;
}
