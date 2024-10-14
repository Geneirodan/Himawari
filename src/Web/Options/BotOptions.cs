namespace Himawari.Web.Options;

public class BotOptions
{
    public string Token { get; init; } = null!;
    public int ApiId { get; init; }
    public string ApiHash { get; init; } = null!;
}