﻿using System.Text;
using System.Text.Json;

namespace Fauna;

public sealed class QueryLiteral : IQueryFragment
{
    public QueryLiteral(string v)
    {
        if (v == null)
        {
            throw new ArgumentNullException(nameof(v), "Value cannot be null.");
        }

        Unwrap = v;
    }

    public string Unwrap { get; }

    public override string ToString()
    {
        return $"QueryLiteral({Unwrap})";
    }

    public void Serialize(Stream stream)
    {
        stream.Write(Encoding.UTF8.GetBytes($"\"{Unwrap}\""));
        stream.Flush();
    }

    public override bool Equals(object? other)
    {
        var otherQuery = other as IQueryFragment;

        if (otherQuery is null)
        {
            return false;
        }

        if (ReferenceEquals(this, otherQuery))
        {
            return true;
        }

        if (otherQuery is QueryLiteral otherLiteral)
        {
            return Unwrap == otherLiteral.Unwrap;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Unwrap.GetHashCode();
    }

    public static bool operator ==(QueryLiteral left, QueryLiteral right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(QueryLiteral left, QueryLiteral right)
    {
        return !Equals(left, right);
    }
}
