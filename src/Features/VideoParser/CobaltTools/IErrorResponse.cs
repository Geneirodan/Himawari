using JetBrains.Annotations;

namespace Himawari.VideoParser.CobaltTools;

[PublicAPI]
public interface IErrorResponse
{
    ErrorObject? Error { get; init; }

    [PublicAPI]
    public sealed record ErrorObject(string Code, ErrorObject.ErrorContext? Context)
    {
        [PublicAPI]
        public sealed record ErrorContext(string? Service, int? Limit);
    }
}