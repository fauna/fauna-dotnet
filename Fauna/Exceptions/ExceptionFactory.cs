using System.Net;
using Fauna.Mapping;

namespace Fauna.Exceptions;

public static class ExceptionFactory
{
    public static Exception FromQueryFailure(MappingContext ctx, QueryFailure f)
    {
        var msg = $"{f.StatusCode} ({f.ErrorCode}): {f.Message}{(f.Summary is { Length: > 0 } ? "\n---\n" + f.Summary : "")}";

        return f.ErrorCode switch
        {
            "abort" => new AbortException(msg, f, ctx),
            "bad_gateway" => new BadGatewayException(msg, f),
            "contended_transaction" => new ContendedTransactionException(msg, f),
            "forbidden" => new ForbiddenException(msg, f),
            "invalid_argument" => new QueryRuntimeException(msg, f),
            "invalid_query" or
                "invalid_function_definition" or
                "invalid_identifier" or
                "invalid_syntax" or
                "invalid_type" => new QueryCheckException(msg, f),
            "invalid_request" => new InvalidRequestException(msg, f),
            "limit_exceeded" => new ThrottlingException(msg, f),
            "time_limit_exceeded" => new QueryTimeoutException(msg, f),
            "time_out" or
                "gateway_timeout" => new TimeoutException(msg, f),
            "unauthorized" => new UnauthorizedException(msg, f),

            _ => new ServiceException(msg, f)
        };
    }


    public static Exception FromRawResponse(string body, HttpResponseMessage r)
    {
        if (r.StatusCode is >= HttpStatusCode.OK and <= (HttpStatusCode)299)
        {
            // We should never get here, but if we do it's outside of the expected wire protocol.
            return new ProtocolException("Malformed response.", r.StatusCode, body);
        }

        return r.StatusCode switch
        {
            HttpStatusCode.TooManyRequests => new ThrottlingException($"{r.StatusCode}: {r.ReasonPhrase ?? "Too many requests."}"),
            _ => new FaunaException($"{r.StatusCode}: {r.ReasonPhrase}")
        };
    }
}
