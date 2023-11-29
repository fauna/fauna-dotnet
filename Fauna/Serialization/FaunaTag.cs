namespace Fauna.Serialization;

public readonly struct FaunaTag
{
    private FaunaTag(string value) { _value = value; }

    private readonly string _value;
    
    public override string ToString() => _value;

    public static implicit operator string(FaunaTag tag) { return tag._value; }

    public static FaunaTag Date => new("@date");
    public static FaunaTag Document => new("@doc");
    public static FaunaTag Double => new("@double");
    public static FaunaTag Int => new("@int");
    public static FaunaTag Long => new("@long");
    public static FaunaTag Module => new("@mod");
    public static FaunaTag Object => new("@object");
    public static FaunaTag Ref => new("@ref");
    public static FaunaTag Set => new("@set");
    public static FaunaTag Time => new("@time");
}
