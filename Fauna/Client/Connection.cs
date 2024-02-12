using Fauna.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Polly;
using AuthenticationException = System.Security.Authentication.AuthenticationException;

namespace Fauna;

/// <summary>
/// An HTTP Client wrapper.
/// </summary>
public class Connection : IConnection
{
    private readonly Configuration _cfg;

    /// <summary>
    /// Initializes a new instance of the Connection class.
    /// </summary>
    /// <param name="configuration">The client configuration to use.</param>
    public Connection(Configuration configuration)
    {
        _cfg = configuration;
    }

    /// <summary>
    /// Asynchronously sends a POST request to the specified path with the provided body and headers.
    /// Implements retry logic with exponential backoff and handles various HTTP and network-related exceptions.
    /// </summary>
    /// <typeparam name="T">The type of the response expected from the request.</typeparam>
    /// <param name="path">The path of the resource to send the request to.</param>
    /// <param name="body">The stream containing the request body.</param>
    /// <param name="headers">A dictionary of headers to be included in the request.</param>
    /// <param name="cancel">A cancellation token to use.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the response from the server as <see cref="HttpResponseMessage"/>.</returns>
    /// <exception cref="ClientException">Thrown when client-side errors occur before sending the request to Fauna.</exception>
    /// <exception cref="NetworkException">Thrown for failures in network communication between the client and Fauna service.</exception>
    /// <exception cref="AuthenticationException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ProtocolException">Thrown when response parsing fails.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public async Task<HttpResponseMessage> DoPostAsync(
        string path,
        Stream body,
        Dictionary<string, string> headers,
        CancellationToken cancel = default)
    {
        string FormatMessage(string errorType, string message) => $"{errorType}: {message}";

        try
        {
            HttpResponseMessage response;
            {

                var policyResult = await _cfg.RetryConfiguration.RetryPolicy
                    .ExecuteAndCaptureAsync(async () => await _cfg.HttpClient.SendAsync(CreateHttpRequest(path, body, headers), cancel))
                    .ConfigureAwait(false);

                if (policyResult.Outcome == OutcomeType.Successful)
                {
                    response = policyResult.Result;
                }
                else
                {
                    throw policyResult.FinalException;
                }
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            throw new NetworkException(FormatMessage("Network Error", ex.Message), ex);
        }
        catch (TaskCanceledException ex)
        {
            if (!ex.CancellationToken.IsCancellationRequested)
            {
                throw new ClientException(FormatMessage("Operation Canceled", ex.Message), ex);
            }
            else
            {
                throw new ClientException(FormatMessage("Operation Timed Out", ex.Message), ex);
            }
        }
        catch (ArgumentNullException ex)
        {
            throw new ClientException(FormatMessage("Null Argument", ex.Message), ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new ClientException(FormatMessage("Invalid Operation", ex.Message), ex);
        }
        catch (JsonException ex)
        {
            throw new ProtocolException(FormatMessage("Response Parsing Failed", ex.Message), ex);
        }
        catch (AuthenticationException ex)
        {
            throw new ClientException(FormatMessage("Authentication Failed", ex.Message), ex);
        }
        catch (NotSupportedException ex)
        {
            throw new ClientException(FormatMessage("Not Supported Operation", ex.Message), ex);
        }
        catch (Exception ex)
        {
            throw new FaunaException(FormatMessage("Unexpected Error", ex.Message), ex);
        }
    }

    private HttpRequestMessage CreateHttpRequest(string path, Stream body, Dictionary<string, string> headers)
    {
        body.Position = 0;
        var request = new HttpRequestMessage
        {
            Content = new StreamContent(body),
            Method = HttpMethod.Post,
            RequestUri = new Uri(_cfg.Endpoint, path)
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        return request;
    }
}
