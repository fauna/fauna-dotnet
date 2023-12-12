namespace Fauna;

public class FaunaException : Exception
{
    public QueryFailure QueryFailure { get; init; }

    public FaunaException(QueryFailure queryFailure, string? message = null) : base(message)
    {
        QueryFailure = queryFailure;
    }
}
