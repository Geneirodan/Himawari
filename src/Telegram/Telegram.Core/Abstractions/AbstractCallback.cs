using Himawari.Telegram.Core.Abstractions.Messages;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions;

public abstract record AbstractCallback(CallbackQuery Query) : ICallback
{
    public Message? Message => Query.Message;
}

public abstract record AbstractCallback<T>(CallbackQuery Query) : ICallback<T>
{
    public Message? Message => Query.Message;
}