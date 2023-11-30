using System.Transactions;

namespace Fauna.Serialization;

public class SerializationException: Exception
{
    public SerializationException(string? message) : base(message)
    {
    }
}