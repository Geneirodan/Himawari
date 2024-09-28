using Himawari.Enums;
using Telegram.Bot.Types;

namespace Himawari.Models;

public record CommandInfo(Command Type, BotCommand BotCommand, string Locale);