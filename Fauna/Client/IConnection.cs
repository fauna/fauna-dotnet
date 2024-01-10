namespace Fauna;

/// <summary>
/// Represents an interface for connections to a Fauna database.
/// </summary>
public interface IConnection
{
    /// <summary>
    /// Asynchronously sends a POST request to the specified path with the provided body and headers.
    /// </summary>
    /// <param name="path">The path of the resource to send the request to.</param>
    /// <param name="body">The stream containing the request body.</param>
    /// <param name="headers">A dictionary of headers to be included in the request.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the response from the server as <see cref="HttpResponseMessage"/>.</returns>
    Task<HttpResponseMessage> DoPostAsync(
        string path,
        Stream body,
        Dictionary<string, string> headers);
}
