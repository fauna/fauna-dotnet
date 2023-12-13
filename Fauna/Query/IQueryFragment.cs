namespace Fauna;

/// <summary>
/// Represents the base interface for a query fragment used for FQL query construction.
/// </summary>
public interface IQueryFragment
{
    /// <summary>
    /// Serializes the query fragment into a string format.
    /// </summary>
    /// <returns>A string representation of the query fragment.</returns>
    void Serialize(Stream stream);
    string Serialize();
}