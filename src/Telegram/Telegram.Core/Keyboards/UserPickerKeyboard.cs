using JetBrains.Annotations;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Himawari.Telegram.Core.Keyboards;

/// <summary>
/// Builds an inline keyboard for selecting a chat member. Each button displays the user's name
/// and sends a callback with the encoded command and payload (e.g. <c>who:userId|label</c>).
/// </summary>
/// <remarks>
/// Callback data is limited to 64 bytes; the label is truncated if necessary.
/// <seealso cref="InlineKeyboardMarkup"/>
/// </remarks>
[PublicAPI]
public static class UserPickerKeyboard
{
    private const int MaxCallbackDataBytes = 64;
    private const int MaxUsersPerRow = 3;
    private const int MaxRows = 5;

    /// <summary>
    /// Builds an inline keyboard from <paramref name="users"/>.
    /// Callback data format: <c>{command}:{userId}|{label}</c> (e.g. "who:123456789|Maria S."); label is truncated to fit 64 bytes.
    /// </summary>
    /// <param name="users">Chat members (non-bot users) to show as buttons.</param>
    /// <param name="command">Command prefix for callback data (e.g. "who", "gift").</param>
    /// <returns>Markup with up to <see cref="MaxRows"/>×<see cref="MaxUsersPerRow"/> buttons.</returns>
    public static InlineKeyboardMarkup Build(IEnumerable<User> users, string command)
    {
        var list = users.Take(MaxUsersPerRow * MaxRows).ToList();
        var buttons = list
            .Select(u =>
            {
                var label = FormatName(u);
                var callbackData = $"{command}:{u.Id}|{label}";
                if (callbackData.Length > MaxCallbackDataBytes)
                {
                    var maxLabelLen = MaxCallbackDataBytes - command.Length - 2 - u.Id.ToString(System.Globalization.CultureInfo.InvariantCulture).Length - 1;
                    label = label.Length <= maxLabelLen ? label : label[..maxLabelLen];
                    callbackData = $"{command}:{u.Id}|{label}";
                }
                return InlineKeyboardButton.WithCallbackData(label, callbackData);
            })
            .Chunk(MaxUsersPerRow)
            .Select(row => row.AsEnumerable())
            .ToArray();

        return new InlineKeyboardMarkup(buttons);
    }

    /// <summary>
    /// Formats the user's display name (first name + first letter of last name if present).
    /// </summary>
    public static string FormatName(User u)
    {
        var name = u.FirstName ?? string.Empty;
        if (!string.IsNullOrEmpty(u.LastName))
            name += $" {u.LastName[0]}.";
        return name;
    }
}
