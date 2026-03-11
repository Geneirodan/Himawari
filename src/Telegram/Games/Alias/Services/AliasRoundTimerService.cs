using System.Threading;

namespace Himawari.Alias.Services;

/// <summary>
/// Default implementation of <see cref="IAliasRoundTimer"/>: holds one <see cref="CancellationTokenSource"/> per chat and runs the elapsed callback after the duration.
/// </summary>
public sealed class AliasRoundTimerService : IAliasRoundTimer
{
    private readonly Dictionary<long, (CancellationTokenSource Cts, Task Task)> _timers = [];
    private readonly Lock _lock = new();

    /// <inheritdoc />
    public void Start(long chatId, TimeSpan duration, Func<long, CancellationToken, Task> onElapsed)
    {
        Cancel(chatId);
        var cts = new CancellationTokenSource();
        var task = RunTimerAsync(chatId, duration, cts, onElapsed);
        lock (_lock)
        {
            _timers[chatId] = (cts, task);
        }
    }

    /// <inheritdoc />
    public void Cancel(long chatId)
    {
        lock (_lock)
        {
            if (!_timers.Remove(chatId, out var pair))
                return;
            pair.Cts.Cancel();
            // Do not Dispose here — the task's finally will run and dispose this CTS.
        }
    }

    private async Task RunTimerAsync(long chatId, TimeSpan duration, CancellationTokenSource cts, Func<long, CancellationToken, Task> onElapsed)
    {
        try
        {
            await Task.Delay(duration, cts.Token).ConfigureAwait(false);
            await onElapsed(chatId, default).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Cancel() was called — exit normally; finally will dispose.
        }
        finally
        {
            lock (_lock)
            {
                if (_timers.TryGetValue(chatId, out var current) && current.Cts == cts)
                    _timers.Remove(chatId);
            }
            cts.Dispose();
        }
    }
}
