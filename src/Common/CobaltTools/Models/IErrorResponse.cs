using JetBrains.Annotations;

namespace Himawari.CobaltTools.Models;

[PublicAPI]
public interface IErrorResponse
{
    ErrorObject? Error { get; init; }

    [PublicAPI]
    sealed record ErrorObject(string Code, ErrorObject.ErrorContext? Context)
    {
        [PublicAPI]
        public sealed record ErrorContext(string? Service, int? Limit);
    }
}