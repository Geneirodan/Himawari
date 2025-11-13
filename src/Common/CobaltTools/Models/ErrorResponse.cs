using JetBrains.Annotations;
using static Himawari.CobaltTools.Models.ErrorResponse;

namespace Himawari.CobaltTools.Models;

[PublicAPI]
public sealed record ErrorResponse(ErrorObject Error) : CobaltToolsResponse
{
    public sealed record ErrorObject(string Code, ErrorObject.ErrorContext? Context)
    {
        [PublicAPI]
        public sealed record ErrorContext(string? Service, int? Limit);
    }
}