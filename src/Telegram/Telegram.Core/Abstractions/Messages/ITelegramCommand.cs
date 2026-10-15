using Mediator;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions.Messages;

public interface ITelegramCommand : ITelegramMessage, ICommand<Message>;
