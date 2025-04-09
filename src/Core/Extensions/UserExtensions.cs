using System.Text;
using Telegram.Bot.Types;

namespace Himawari.Core.Extensions;

public static class UserExtensions
{
    public static string GetDisplayName(this User user)
    {
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

    public static string GetUsername(this User user) => 
        user.Username is not null ? $"@{user.Username}" : user.GetDisplayName();
}