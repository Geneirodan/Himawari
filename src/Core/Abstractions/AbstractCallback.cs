using Himawari.Core.Abstractions.Messages;
using Telegram.Bot.Types;

namespace Himawari.Core.Abstractions;

public abstract record AbstractCallback(CallbackQuery Query) : ICallback
{
    public Message? Message => Query.Message;
}

public abstract record AbstractCallback<T>(CallbackQuery Query) : ICallback<T>
{
    public Message? Message => Query.Message;
}