using Himawari.Telegram.Core.Abstractions.Messages;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions;

/// <summary>
/// Base type for Telegram callback query handlers that carry the <see cref="CallbackQuery"/> and expose the optional <see cref="Message"/>.
/// </summary>
/// <param name="Query">The callback query from the update.</param>
public abstract record AbstractCallback(CallbackQuery Query) : ICallback
{
    /// <inheritdoc />
    public Message? Message => Query.Message;
}

/// <summary>
/// Base type for Telegram callback query handlers that return a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The response type.</typeparam>
/// <param name="Query">The callback query from the update.</param>
public abstract record AbstractCallback<T>(CallbackQuery Query) : ICallback<T>
{
    /// <inheritdoc />
    public Message? Message => Query.Message;
}