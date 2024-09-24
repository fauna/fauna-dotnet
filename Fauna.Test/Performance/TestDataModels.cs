using Fauna.Mapping;

namespace Fauna.Test.Performance;

/// <summary>
/// Representation of the Product collection as defined in './setup/fauna/main.fsl'
/// </summary>
internal class Product
{
    [Field]
    public string? Name { get; init; }

    [Field]
    public string? Category { get; init; }

    [Field]
    public int Price { get; init; } = 0;

    [Field]
    public int Quantity { get; init; } = 0;

    [Field]
    public bool InStock { get; init; } = false;
}

/// <summary>
/// Representation of the Manufacturer collection as defined in './setup/fauna/main.fsl'
/// </summary>
internal class Manufacturer
{
    [Field]
    public string? Name { get; init; }

    [Field]
    public string? Location { get; init; }
}
