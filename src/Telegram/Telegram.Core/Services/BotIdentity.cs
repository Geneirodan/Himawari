using JetBrains.Annotations;
using WTelegram;

namespace Himawari.Telegram.Core.Services;

/// <summary>
/// Lazy-cached bot identity: resolves and caches the bot username from <c>GetMe</c> so that group NL parsing and slash-command @mention checks do not trigger an HTTP call per message.
/// Uses <see cref="Lazy{T}"/> with <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/> so that at most one <c>GetMe</c> runs even under high concurrency.
/// </summary>
[PublicAPI]
public sealed class BotIdentity(Bot bot) : IBotIdentity
{
    private readonly Lazy<Task<string?>> _username = new(
        async () => (await bot.GetMe().ConfigureAwait(false))?.Username,
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <inheritdoc />
    public ValueTask<string?> GetUsernameAsync(CancellationToken cancellationToken = default)
        => new(_username.Value);
}
