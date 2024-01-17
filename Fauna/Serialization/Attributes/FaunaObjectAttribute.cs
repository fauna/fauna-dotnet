namespace Fauna.Serialization.Attributes;

/// <summary>
/// Attribute used to indicate that a class represents a Fauna object.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class FaunaObjectAttribute : Attribute
{
}