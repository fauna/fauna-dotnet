using Fauna.Exceptions;
using Fauna.Serialization;
using Fauna.Types;
using Fauna.Mapping;
using System.Diagnostics.CodeAnalysis;

namespace Fauna;

/// <summary>
/// Represents a client for interacting with a Fauna.
/// </summary>
interface IClient
{
    /// <summary>
    /// Asynchronously executes a specified FQL query against the Fauna database and returns the typed result.
    /// </summary>
    /// <typeparam name="T">The type of the result expected from the query, corresponding to the structure of the FQL query's expected response.</typeparam>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the result of the query execution as <see cref="QuerySuccess{T}"/>.</returns>
    /// <exception cref="ClientException">Thrown when client-side errors occur before sending the request to Fauna.</exception>
    /// <exception cref="AuthenticationException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="AuthorizationException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="NetworkException">Thrown for failures in network communication between the client and Fauna service.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public Task<QuerySuccess<T>> QueryAsync<T>(
        Query query,
        QueryOptions? queryOptions = null)
        where T : notnull;

    /// <summary>
    /// Asynchronously executes a specified FQL query against the Fauna database.
    /// </summary>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the result of the query execution.</returns>
    /// <exception cref="ClientException">Thrown when client-side errors occur before sending the request to Fauna.</exception>
    /// <exception cref="AuthenticationException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="AuthorizationException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="NetworkException">Thrown for failures in network communication between the client and Fauna service.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public Task<QuerySuccess<object?>> QueryAsync(
        Query query,
        QueryOptions? queryOptions = null);

    /// <summary>
    /// Asynchronously executes a specified FQL query against the Fauna database and returns the typed result.
    /// </summary>
    /// <typeparam name="T">The type of the result expected from the query, corresponding to the structure of the FQL query's expected response.</typeparam>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="deserializer">A deserializer for the success data type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the result of the query execution as <see cref="QuerySuccess{T}"/>.</returns>
    /// <exception cref="ClientException">Thrown when client-side errors occur before sending the request to Fauna.</exception>
    /// <exception cref="AuthenticationException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="AuthorizationException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="NetworkException">Thrown for failures in network communication between the client and Fauna service.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public Task<QuerySuccess<T>> QueryAsync<T>(
        Query query,
        IDeserializer<T> deserializer,
        QueryOptions? queryOptions = null);

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// </summary>
    /// <typeparam name="T">The type of the data expected in each page.</typeparam>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <returns>An asynchronous enumerable of pages, each containing a list of items of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This method handles pagination by sending multiple requests to Fauna as needed, based on the presence of an 'after' cursor in the query results.
    /// </remarks>
    /// <exception cref="ClientException">Thrown when client-side errors occur before sending the request to Fauna.</exception>
    /// <exception cref="AuthenticationException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="AuthorizationException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="NetworkException">Thrown for failures in network communication between the client and Fauna service.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Query query,
        QueryOptions? queryOptions = null)
        where T : notnull;

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// </summary>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <returns>An asynchronous enumerable of pages, each containing a list of items.</returns>
    /// <remarks>
    /// This method handles pagination by sending multiple requests to Fauna as needed, based on the presence of an 'after' cursor in the query results.
    /// </remarks>
    /// <exception cref="ClientException">Thrown when client-side errors occur before sending the request to Fauna.</exception>
    /// <exception cref="AuthenticationException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="AuthorizationException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="NetworkException">Thrown for failures in network communication between the client and Fauna service.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Query query,
        QueryOptions? queryOptions = null);

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// </summary>
    /// <typeparam name="T">The type of the data expected in each page.</typeparam>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="elemDeserializer">A data deserializer for the page element type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <returns>An asynchronous enumerable of pages, each containing a list of items of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This method handles pagination by sending multiple requests to Fauna as needed, based on the presence of an 'after' cursor in the query results.
    /// </remarks>
    /// <exception cref="ClientException">Thrown when client-side errors occur before sending the request to Fauna.</exception>
    /// <exception cref="AuthenticationException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="AuthorizationException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="NetworkException">Thrown for failures in network communication between the client and Fauna service.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Query query,
        IDeserializer<T> elemDeserializer,
        QueryOptions? queryOptions = null);
}

/// <summary>
/// The base class for Client and DatabaseContext.
/// </summary>
public abstract class BaseClient : IClient
{
    internal BaseClient() { }

    protected abstract MappingContext MappingCtx { get; }

    internal abstract Task<QuerySuccess<T>> QueryAsyncInternal<T>(
        Query query,
        IDeserializer<T> deserializer,
        MappingContext ctx,
        QueryOptions? queryOptions
    );

    #region IClient

    public Task<QuerySuccess<T>> QueryAsync<T>(
        Query query,
        QueryOptions? queryOptions = null)
        where T : notnull =>
    QueryAsync<T>(query, Deserializer.Generate<T>(MappingCtx), queryOptions);

    public Task<QuerySuccess<object?>> QueryAsync(
        Query query,
        QueryOptions? queryOptions = null) =>
        QueryAsync<object?>(query, Deserializer.Dynamic, queryOptions);

    public Task<QuerySuccess<T>> QueryAsync<T>(
        Query query,
        IDeserializer<T> deserializer,
        QueryOptions? queryOptions = null) =>
        QueryAsyncInternal<T>(query, deserializer, MappingCtx, queryOptions);

    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Query query,
        QueryOptions? queryOptions = null)
        where T : notnull =>
        PaginateAsync(query, Deserializer.Generate<T>(MappingCtx), queryOptions);

    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Query query,
        QueryOptions? queryOptions = null) =>
        PaginateAsync(query, Deserializer.Dynamic, queryOptions);

    public async IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Query query,
        IDeserializer<T> elemDeserializer,
        QueryOptions? queryOptions = null)
    {
        Page<T>? currentPage = null;

        var deserializer = new PageDeserializer<T>(elemDeserializer);

        do
        {
            var currentQuery = currentPage?.After is not null
                ? new QueryExpr(new QueryLiteral($"Set.paginate('{currentPage.After}')"))
                : query;

            var response = await QueryAsyncInternal<Page<T>>(currentQuery,
                                                             deserializer,
                                                             MappingCtx,
                                                             queryOptions);

            if (response.Data is not null)
            {
                currentPage = response.Data;
                yield return currentPage;
            }
            else
            {
                throw new FaunaException("Unexpected response received.");
            }
        } while (currentPage?.After is not null);
    }

    #endregion
}
