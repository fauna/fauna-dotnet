using System.Runtime.CompilerServices;
using Fauna.Exceptions;
using Fauna.Serialization;
using Fauna.Types;
using Fauna.Mapping;

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
    /// <param name="cancel">A cancellation token to use</param>
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
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
        where T : notnull;

    /// <summary>
    /// Asynchronously executes a specified FQL query against the Fauna database.
    /// </summary>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
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
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously executes a specified FQL query against the Fauna database and returns the typed result.
    /// </summary>
    /// <typeparam name="T">The type of the result expected from the query, corresponding to the structure of the FQL query's expected response.</typeparam>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="deserializer">A deserializer for the success data type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
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
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously executes a specified FQL query against the Fauna database and returns the typed result.
    /// </summary>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="deserializer">A deserializer for the success data type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation toke to use.</param>
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
        IDeserializer deserializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// </summary>
    /// <typeparam name="T">The type of the data expected in each page.</typeparam>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
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
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
        where T : notnull;

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// </summary>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
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
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// </summary>
    /// <typeparam name="T">The type of the data expected in each page.</typeparam>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="elemDeserializer">A data deserializer for the page element type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
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
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// </summary>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="elemDeserializer">A data deserializer for the page element type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the result of the query execution.</returns>
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
        IDeserializer elemDeserializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);
}

/// <summary>
/// The base class for Client and DataContext.
/// </summary>
public abstract class BaseClient : IClient
{
    internal BaseClient() { }

    internal abstract MappingContext MappingCtx { get; }

    internal abstract Task<QuerySuccess<T>> QueryAsyncInternal<T>(
        Query query,
        IDeserializer<T> deserializer,
        MappingContext ctx,
        QueryOptions? queryOptions,
        CancellationToken cancel
    );

    #region IClient

    public Task<QuerySuccess<T>> QueryAsync<T>(
        Query query,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
        where T : notnull =>
        QueryAsync<T>(query, Deserializer.Generate<T>(MappingCtx), queryOptions, cancel);

    public Task<QuerySuccess<object?>> QueryAsync(
        Query query,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default) =>
        QueryAsync<object?>(query, Deserializer.Dynamic, queryOptions, cancel);

    public Task<QuerySuccess<T>> QueryAsync<T>(
        Query query,
        IDeserializer<T> deserializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default) =>
        QueryAsyncInternal(query, deserializer, MappingCtx, queryOptions, cancel);

    public Task<QuerySuccess<object?>> QueryAsync(
        Query query,
        IDeserializer deserializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default) =>
        QueryAsync<object?>(query, (IDeserializer<object?>)deserializer, queryOptions, cancel);

    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Query query,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
        where T : notnull =>
        PaginateAsync(query, Deserializer.Generate<T>(MappingCtx), queryOptions, cancel);

    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Query query,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default) =>
        PaginateAsync(query, Deserializer.Dynamic, queryOptions, cancel);

    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Query query,
        IDeserializer<T> elemDeserializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
    {
        var deserializer = new PageDeserializer<T>(elemDeserializer);
        return PaginateAsyncInternal(query, deserializer, queryOptions, cancel);
    }

    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Query query,
        IDeserializer elemDeserializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
    {
        var elemObjDeser = (IDeserializer<object?>)elemDeserializer;
        var deserializer = new PageDeserializer<object?>(elemObjDeser);
        return PaginateAsyncInternal(query, deserializer, queryOptions, cancel);
    }

    #endregion

    // Internally accessible for QuerySource use
    internal async IAsyncEnumerable<Page<T>> PaginateAsyncInternal<T>(
        Query query,
        PageDeserializer<T> deserializer,
        QueryOptions? queryOptions,
        [EnumeratorCancellation] CancellationToken cancel = default)
    {
        Page<T>? currentPage = null;

        do
        {
            var currentQuery = currentPage?.After is not null
                ? new QueryExpr(new QueryLiteral($"Set.paginate('{currentPage.After}')"))
                : query;

            var response = await QueryAsyncInternal(currentQuery,
                                                    deserializer,
                                                    MappingCtx,
                                                    queryOptions,
                                                    cancel);

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
}
