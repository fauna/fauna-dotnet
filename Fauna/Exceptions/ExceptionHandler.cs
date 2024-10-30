using System.Net;
using Fauna.Core;
using Fauna.Mapping;

namespace Fauna.Exceptions;

/// <summary>
/// A utility class for generating an appropriate <see cref="FaunaException"/> from a <see cref="QueryFailure"/>.
/// </summary>
public static class ExceptionHandler
{
    /// <summary>
    /// Creates an exception from a <see cref="QueryFailure"/>
    /// </summary>
    /// <param name="ctx">A <see cref="MappingContext"/> used for exceptions that require additional deserialization, such as <see cref="AbortException"/>.</param>
    /// <param name="f">The <see cref="QueryFailure"/>.</param>
    /// <returns></returns>
    public static Exception FromQueryFailure(MappingContext ctx, QueryFailure f)
    {
        var msg =
            $"{f.StatusCode} ({f.ErrorCode}): {f.Message}{(f.Summary is { Length: > 0 } ? "\n---\n" + f.Summary : "")}";

        return f.ErrorCode switch
        {
            "abort" => new AbortException(msg, f, ctx),
            "bad_gateway" => new BadGatewayException(msg, f),
            "contended_transaction" => new ContendedTransactionException(msg, f),
            "forbidden" => new AuthorizationException(msg, f),
            "internal_error" => new ServiceException(msg, f),
            "invalid_query" => new QueryCheckException(msg, f),
            "invalid_request" => new InvalidRequestException(msg, f),
            "limit_exceeded" => new ThrottlingException(msg, f),
            "time_out" => new QueryTimeoutException(msg, f),
            "gateway_timeout" => new NetworkException(msg, f.StatusCode, f.Message),
            "unauthorized" => new AuthenticationException(msg, f),
            "constraint_failure" => new ConstraintFailureException(msg, f),

            _ => new QueryRuntimeException(msg, f)
        };
    }


    /// <summary>
    /// Creates an exception from a body and an already consumed <see cref="HttpResponseMessage"/>
    /// </summary>
    /// <param name="body">The response body.</param>
    /// <param name="r">The <see cref="HttpResponseMessage"/> with consumed body.</param>
    /// <returns></returns>
    public static Exception FromRawResponse(string body, HttpResponseMessage r)
    {
        if (r.StatusCode is >= HttpStatusCode.OK and <= (HttpStatusCode)299)
        {
            // We should never get here, but if we do it's outside of the expected wire protocol.
            return new ProtocolException("Malformed response.", r.StatusCode, body);
        }

        return r.StatusCode switch
        {
            HttpStatusCode.TooManyRequests => new ThrottlingException(
                $"{r.StatusCode}: {r.ReasonPhrase ?? "Too many requests."}"),
            _ => new FaunaException($"{r.StatusCode}: {r.ReasonPhrase}")
        };
    }
}
