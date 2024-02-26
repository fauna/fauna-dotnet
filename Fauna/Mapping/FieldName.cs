namespace Fauna.Mapping;

/// <summary>
/// A class of utilities for field names.
/// </summary>
public static class FieldName
{
    /// <summary>
    /// The canonical representation of a field name in Faun. C# properties are capitalized whereas Fauna fields are not by convention.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <returns>The canonical name for the field in Fauna.</returns>
    public static string Canonical(string name) =>
        (string.IsNullOrEmpty(name) || char.IsLower(name[0])) ?
            name :
            string.Concat(name[0].ToString().ToLower(), name.AsSpan(1));
}
