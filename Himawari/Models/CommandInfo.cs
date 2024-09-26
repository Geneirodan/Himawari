using System.Globalization;
using Telegram.Bot.Types;

namespace Himawari.Services;

public record CommandInfo(CommandType Type, BotCommand Command, CultureInfo CultureInfo);