using JetBrains.Annotations;
using static Himawari.CobaltTools.Models.ErrorResponse;

namespace Himawari.CobaltTools.Models;

/// <summary>
/// CobaltTools API error response containing a code and optional context (service name, limit).
/// </summary>
/// <param name="Error">The error payload with <see cref="ErrorObject.Code"/> and optional <see cref="ErrorObject.Context"/>.</param>
[PublicAPI]
public sealed record ErrorResponse(ErrorObject Error) : CobaltToolsResponse
{
    /// <summary>
    /// Error payload returned by the CobaltTools API.
    /// </summary>
    /// <param name="Code">Error code string.</param>
    /// <param name="Context">Optional context (e.g. service name, rate limit).</param>
    public sealed record ErrorObject(string Code, ErrorObject.ErrorContext? Context)
    {
        /// <summary>
        /// Optional error context: service identifier and optional limit.
        /// </summary>
        /// <param name="Service">Service name, if applicable.</param>
        /// <param name="Limit">Numeric limit, if applicable.</param>
        [PublicAPI]
        public sealed record ErrorContext(string? Service, int? Limit);
    }
}