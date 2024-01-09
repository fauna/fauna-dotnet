namespace Fauna.Types;

/// <summary>
/// Represents a page in a dataset for pagination.
/// </summary>
/// <typeparam name="T">The type of data contained in the page.</typeparam>
/// <param name="Data">The <see cref="IReadOnlyList{T}"/> of data items on the page.</param>
/// <param name="After">The cursor for the next page, if available.</param>
/// <remarks>
/// Used for segmenting large datasets into pages with cursors to navigate between them.
/// </remarks>
public record Page<T>(IReadOnlyList<T> Data, string? After);
