namespace Fauna;

/// <summary>
/// Represents the base interface for a query fragment used for FQL query construction.
/// </summary>
public interface IQueryFragment
{
}

/// <summary>
/// Represents a generic interface for a query fragment used for FQL query construction.
/// </summary>
/// <typeparam name="T">The type of the value wrapped by the query fragment.</typeparam>
public interface IQueryFragment<T> : IQueryFragment
{
    /// Gets the wrapped value of the query fragment
    /// </summary>
    T Unwrap { get; }
}