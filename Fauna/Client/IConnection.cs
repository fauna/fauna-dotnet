namespace Fauna;

/// <summary>
/// Represents an interface for making HTTP requests.
/// </summary>
internal interface IConnection : IDisposable
{
    /// <summary>
    /// Asynchronously sends a POST request to the specified path with the provided body and headers.
    /// </summary>
    /// <param name="path">The path of the resource to send the request to.</param>
    /// <param name="body">The stream containing the request body.</param>
    /// <param name="headers">A dictionary of headers to be included in the request.</param>
    /// <param name="cancel">A cancellation token to use with the request.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the response from the server as <see cref="HttpResponseMessage"/>.</returns>
    Task<HttpResponseMessage> DoPostAsync(
        string path,
        Stream body,
        Dictionary<string, string> headers,
        CancellationToken cancel);

    /// <summary>
    /// Asynchronously sends a POST request to open Stream.
    /// </summary>
    /// <param name="path">The path of the resource to send the request to.</param>
    /// <param name="body">The body of the Stream request.</param>
    /// <param name="headers">The headers to include in the request.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, which opens the HTTP stream and returns the <see cref="HttpResponseMessage"/>.</returns>
    Task<HttpResponseMessage> OpenStream(
        string path,
        Stream body,
        Dictionary<string, string> headers,
        CancellationToken cancellationToken = default);
}
