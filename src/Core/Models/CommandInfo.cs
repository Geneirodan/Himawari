using Himawari.Core.Enums;
using Telegram.Bot.Types;

namespace Himawari.Core.Models;

public record CommandInfo(Command Type, BotCommand BotCommand, string Locale);