﻿namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions caused by invalid requests to Fauna.
/// </summary>
public class InvalidRequestException : ServiceException
{
    public InvalidRequestException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }

    public InvalidRequestException(QueryFailure queryFailure, string message, Exception innerException)
        : base(queryFailure, message, innerException) { }
}