using System.Text;
using Telegram.Bot.Types;

namespace Himawari.Core.Extensions;

/// <summary>
/// Provides extension methods for the Telegram.Bot.Types.User class to simplify display and username formatting.
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Returns a Markdown-formatted display name for the user, including a clickable link to their Telegram profile.
    /// If the user's first name is missing, returns null (Deleted account).
    /// </summary>
    public static string? GetDisplayName(this User user)
    {
        if (string.IsNullOrWhiteSpace(user.FirstName))
            return null;
        var builder = new StringBuilder("[")
            .Append(user.FirstName);
        if (user.LastName is not null)
            builder.Append(' ').Append(user.LastName);
        return builder
            .Append("](tg://user?id=")
            .Append(user.Id)
            .Append(')')
            .ToString();
    }

    /// <summary>
    /// Returns the user's username prefixed with '@' if available; otherwise, returns the Markdown-formatted display name.
    /// </summary>
    public static string? GetUsername(this User user)
    {
        return user.Username is not null ? $"@{user.Username}" : user.GetDisplayName();
    }
}