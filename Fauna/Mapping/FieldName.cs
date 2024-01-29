namespace Fauna.Mapping;

public static class FieldName
{
    // C# properties are capitalized whereas Fauna fields are not by convention.
    public static string Canonical(string name) =>
        (string.IsNullOrEmpty(name) || char.IsLower(name[0])) ?
            name :
            string.Concat(name[0].ToString().ToLower(), name.AsSpan(1));
}
