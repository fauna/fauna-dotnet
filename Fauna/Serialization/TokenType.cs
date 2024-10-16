namespace Fauna.Serialization;

/// <summary>
/// Enumerates the types of tokens used in Fauna serialization.
/// </summary>
public enum TokenType
{
    /// <summary>There is no value. This is the default token type if no data has been read by the <see cref="T:Fauna.Serialization.Utf8FaunaReader" />.</summary>
    None,

    /// <summary>The token type is the start of a Fauna object.</summary>
    StartObject,
    /// <summary>The token type is the end of a Fauna object.</summary>
    EndObject,

    /// <summary>The token type is the start of a Fauna array.</summary>
    StartArray,
    /// <summary>The token type is the end of a Fauna array.</summary>
    EndArray,

    /// <summary>The token type is the start of a Fauna set (a.k.a. page).</summary>
    StartPage,
    /// <summary>The token type is the end of a Fauna set (a.k.a. page).</summary>
    EndPage,

    /// <summary>The token type is the start of a Fauna ref.</summary>
    StartRef,
    /// <summary>The token type is the end of a Fauna ref.</summary>
    EndRef,

    /// <summary>The token type is the start of a Fauna document.</summary>
    StartDocument,
    /// <summary>The token type is the end of a Fauna document.</summary>
    EndDocument,

    /// <summary>The token type is a Fauna property name.</summary>
    FieldName,

    /// <summary>The token type is a Fauna string.</summary>
    String,

    /// <summary>The token type is a Fauna integer.</summary>
    Int,
    /// <summary>The token type is a Fauna long.</summary>
    Long,
    /// <summary>The token type is a Fauna double.</summary>
    Double,

    /// <summary>The token type is a Fauna date.</summary>
    Date,
    /// <summary>The token type is a Fauna time.</summary>
    Time,

    /// <summary>The token type is the Fauna literal true.</summary>
    True,
    /// <summary>The token type is the Fauna literal false.</summary>
    False,

    /// <summary>The token type is the Fauna literal null.</summary>
    Null,

    /// <summary>The token type is the Fauna module.</summary>
    Module,

    /// <summary>The token type is the Fauna stream token.</summary>
    Stream,
}

public static class TokenTypeExtensions
{
    public static FaunaType GetFaunaType(this TokenType tokenType)
    {
        switch (tokenType)
        {
            case TokenType.StartObject:
            case TokenType.EndObject:
                return FaunaType.Object;

            case TokenType.StartArray:
            case TokenType.EndArray:
                return FaunaType.Array;

            case TokenType.StartPage:
            case TokenType.EndPage:
                return FaunaType.Set;

            case TokenType.StartRef:
            case TokenType.EndRef:
                return FaunaType.Ref;

            case TokenType.StartDocument:
            case TokenType.EndDocument:
                return FaunaType.Document;

            case TokenType.String:
                return FaunaType.String;
            // BUGBUG: @bytes support is missing; BT-5183
            // case TokenType.Bytes:
            //     return FaunaType.Bytes;
            case TokenType.Int:
                return FaunaType.Int;
            case TokenType.Long:
                return FaunaType.Long;
            case TokenType.Double:
                return FaunaType.Double;
            case TokenType.Date:
                return FaunaType.Date;
            case TokenType.Time:
                return FaunaType.Time;
            case TokenType.True:
            case TokenType.False:
                return FaunaType.Boolean;
            case TokenType.Null:
                return FaunaType.Null;
            case TokenType.Stream:
                return FaunaType.Stream;
            case TokenType.Module:
                return FaunaType.Module;
            default:
                throw new ArgumentException($"No associated FaunaType for TokenType: {tokenType}");
        }
    }
}
