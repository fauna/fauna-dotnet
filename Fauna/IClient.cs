using System.Runtime.CompilerServices;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Protocol;
using Fauna.QueryFragments;
using Fauna.Serialization;
using Fauna.Serialization.Serializers;
using Fauna.Types;
using Stream = Fauna.Types.Stream;

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
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
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
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
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
    /// <param name="serializer">A serializer for the success data type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the result of the query execution as <see cref="QuerySuccess{T}"/>.</returns>
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public Task<QuerySuccess<T>> QueryAsync<T>(
        Query query,
        ISerializer<T> serializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously executes a specified FQL query against the Fauna database and returns the typed result.
    /// </summary>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="serializer">A serializer for the success data type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation toke to use.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the result of the query execution.</returns>
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public Task<QuerySuccess<object?>> QueryAsync(
        Query query,
        ISerializer serializer,
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
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Query query,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
        where T : notnull;

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// The provided page is the first page yielded.
    /// </summary>
    /// <typeparam name="T">The type of the data expected in each page.</typeparam>
    /// <param name="page">The initial page.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
    /// <returns>An asynchronous enumerable of pages, each containing a list of items of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This method handles pagination by sending multiple requests to Fauna as needed, based on the presence of an 'after' cursor in the query results.
    /// </remarks>
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Page<T> page,
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
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Query query,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// The provided page is the first page yielded.
    /// </summary>
    /// <param name="page">The initial page.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
    /// <returns>An asynchronous enumerable of pages, each containing a list of items.</returns>
    /// <remarks>
    /// This method handles pagination by sending multiple requests to Fauna as needed, based on the presence of an 'after' cursor in the query results.
    /// </remarks>
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Page<object?> page,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// </summary>
    /// <typeparam name="T">The type of the data expected in each page.</typeparam>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="elemSerializer">A data serializer for the page element type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
    /// <returns>An asynchronous enumerable of pages, each containing a list of items of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This method handles pagination by sending multiple requests to Fauna as needed, based on the presence of an 'after' cursor in the query results.
    /// </remarks>
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Query query,
        ISerializer<T> elemSerializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// The provided page is the first page yielded.
    /// </summary>
    /// <typeparam name="T">The type of the data expected in each page.</typeparam>
    /// <param name="page">The initial page.</param>
    /// <param name="elemSerializer">A data serializer for the page element type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
    /// <returns>An asynchronous enumerable of pages, each containing a list of items of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This method handles pagination by sending multiple requests to Fauna as needed, based on the presence of an 'after' cursor in the query results.
    /// </remarks>
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Page<T> page,
        ISerializer<T> elemSerializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// </summary>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="elemSerializer">A data serializer for the page element type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the result of the query execution.</returns>
    /// <remarks>
    /// This method handles pagination by sending multiple requests to Fauna as needed, based on the presence of an 'after' cursor in the query results.
    /// </remarks>
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Query query,
        ISerializer elemSerializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default);

    /// <summary>
    /// Asynchronously iterates over pages of a Fauna query result, automatically fetching subsequent pages using the 'after' cursor.
    /// The provided page is the first page yielded.
    /// </summary>
    /// <param name="page">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="elemSerializer">A data serializer for the page element type.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <param name="cancel">A cancellation token to use.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the result of the query execution.</returns>
    /// <remarks>
    /// This method handles pagination by sending multiple requests to Fauna as needed, based on the presence of an 'after' cursor in the query results.
    /// </remarks>
    /// <exception cref="UnauthorizedException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="ForbiddenException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Page<object?> page,
        ISerializer elemSerializer,
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
        ISerializer<T> serializer,
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
        QueryAsync<T>(query, Serializer.Generate<T>(MappingCtx), queryOptions, cancel);

    public Task<QuerySuccess<object?>> QueryAsync(
        Query query,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default) =>
        QueryAsync<object?>(query, Serializer.Dynamic, queryOptions, cancel);

    public Task<QuerySuccess<T>> QueryAsync<T>(
        Query query,
        ISerializer<T> serializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default) =>
        QueryAsyncInternal(query, serializer, MappingCtx, queryOptions, cancel);

    public Task<QuerySuccess<object?>> QueryAsync(
        Query query,
        ISerializer serializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default) =>
        QueryAsync<object?>(query, (ISerializer<object?>)serializer, queryOptions, cancel);

    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Query query,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
        where T : notnull =>
        PaginateAsync(query, Serializer.Generate<T>(MappingCtx), queryOptions, cancel);

    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Page<T> page,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
        where T : notnull =>
        PaginateAsync(page, Serializer.Generate<T>(MappingCtx), queryOptions, cancel);

    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Query query,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default) =>
        PaginateAsync(query, Serializer.Dynamic, queryOptions, cancel);

    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Page<object?> page,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default) =>
        PaginateAsync(page, Serializer.Dynamic, queryOptions, cancel);

    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Query query,
        ISerializer<T> elemSerializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
    {
        var serializer = new PageSerializer<T>(elemSerializer);
        return PaginateAsyncInternal(query, serializer, queryOptions, cancel);
    }

    public IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        Page<T> page,
        ISerializer<T> elemSerializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
    {
        var serializer = new PageSerializer<T>(elemSerializer);
        return PaginateAsyncInternal(page, serializer, queryOptions, cancel);
    }

    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Query query,
        ISerializer elemSerializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
    {
        var elemObjSer = (ISerializer<object?>)elemSerializer;
        var serializer = new PageSerializer<object?>(elemObjSer);
        return PaginateAsyncInternal(query, serializer, queryOptions, cancel);
    }

    public IAsyncEnumerable<Page<object?>> PaginateAsync(
        Page<object?> page,
        ISerializer elemSerializer,
        QueryOptions? queryOptions = null,
        CancellationToken cancel = default)
    {
        var elemObjSer = (ISerializer<object?>)elemSerializer;
        var serializer = new PageSerializer<object?>(elemObjSer);
        return PaginateAsyncInternal(page, serializer, queryOptions, cancel);
    }

    #endregion

    // Internally accessible for QuerySource use
    internal async IAsyncEnumerable<Page<T>> PaginateAsyncInternal<T>(
        Query query,
        PageSerializer<T> serializer,
        QueryOptions? queryOptions,
        [EnumeratorCancellation] CancellationToken cancel = default)
    {
        var p = await QueryAsyncInternal(query,
            serializer,
            MappingCtx,
            queryOptions,
            cancel);

        await foreach (var page in PaginateAsyncInternal(p.Data, serializer, queryOptions, cancel))
        {
            yield return page;
        }
    }

    private async IAsyncEnumerable<Page<T>> PaginateAsyncInternal<T>(
        Page<T> page,
        PageSerializer<T> serializer,
        QueryOptions? queryOptions,
        [EnumeratorCancellation] CancellationToken cancel = default)
    {
        yield return page;

        while (page.After is not null)
        {
            var q = new QueryExpr(new QueryLiteral($"Set.paginate('{page.After}')"));

            var response = await QueryAsyncInternal(q,
                serializer,
                MappingCtx,
                queryOptions,
                cancel);

            page = response.Data;
            yield return page;
        }
    }

    #region Streaming

    /// <summary>
    /// Opens the stream with Fauna and returns an enumerator for the stream events.
    /// </summary>
    /// <typeparam name="T">The type of event data that will be deserialized from the stream.</typeparam>
    /// <param name="stream">The stream to subscribe to.</param>
    /// <param name="ctx">The mapping context to use for deserializing stream events.</param>
    /// <param name="cancel">The cancellation token for the operation.</param>
    /// <returns>An async enumerator of stream events.</returns>
    /// Implementation <seealso cref="Client.SubscribeStreamInternal{T}(Stream, MappingContext, CancellationToken)"/>
    internal abstract IAsyncEnumerator<Event<T>> SubscribeStreamInternal<T>(
        Stream stream,
        MappingContext ctx,
        CancellationToken cancel = default) where T : notnull;

    /// <summary>
    /// Retrieves a Stream token from Fauna and returns a StreamEnumerable for the stream events.
    /// </summary>
    /// <typeparam name="T">Event Data will be deserialized to this type.</typeparam>
    /// <param name="query">The query to create the stream from Fauna.</param>
    /// <param name="queryOptions">The options for the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a stream of events.</returns>
    public async Task<StreamEnumerable<T>> EventStreamAsync<T>(
        Query query,
        QueryOptions? queryOptions = null,
        CancellationToken cancellationToken = default) where T : notnull
    {
        var response = await QueryAsync<Stream>(
            query,
            queryOptions,
            cancellationToken);

        return new StreamEnumerable<T>(
            this,
            response.Data,
            cancellationToken);
    }

    /// <summary>
    /// Opens the stream with Fauna and returns an enumerator for the stream events.
    /// </summary>
    /// <typeparam name="T">Event Data will be deserialized to this type.</typeparam>
    /// <param name="stream">The stream to subscribe to.</param>
    /// <param name="ctx">Mapping context for stream.</param?
    /// <param name="cancel">The cancellation token.</param>
    /// <returns>An async enumerator of stream events.</returns>
    public IAsyncEnumerator<Event<T>> SubscribeStream<T>(
        Stream stream,
        MappingContext ctx,
        CancellationToken cancel = default) where T : notnull
    {
        return SubscribeStreamInternal<T>(stream, ctx, cancel);
    }

    #endregion
}
