namespace Himawari.Core.Options;

public sealed record BotOptions
{
    public required string Token { get; init; }
    public required int ApiId { get; init; }
    public required string ApiHash { get; init; }
    public required string[] SupportedLocales { get; init; }
    public required TimeSpan PingTimeout { get; init; }
}