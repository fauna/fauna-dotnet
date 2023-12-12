using System.Collections;

namespace Fauna;

public sealed class QueryVal<T> : Query, IQueryFragment<T>
{
    public QueryVal(T v)
    {
        if (v == null)
        {
            throw new ArgumentNullException(nameof(v), "Value cannot be null.");
        }

        ValidateValue(v);

        Unwrap = v;
    }

    public T Unwrap { get; }

    public override bool Equals(Query? o) => IsEqual(o as QueryVal<T>);

    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o))
        {
            return true;
        }

        if (o is null)
        {
            return false;
        }

        if (GetType() != o.GetType())
        {
            return false;
        }

        return IsEqual(o as QueryVal<T>);
    }

    public override int GetHashCode() => Unwrap != null ? EqualityComparer<T>.Default.GetHashCode(Unwrap) : 0;

    public override string ToString() => $"QueryVal({Unwrap})";

    public static bool operator ==(QueryVal<T> left, QueryVal<T> right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(QueryVal<T> left, QueryVal<T> right)
    {
        return !(left == right);
    }

    private bool IsEqual(QueryVal<T>? o)
    {
        if (o is null)
        {
            return false;
        }

        return EqualityComparer<T>.Default.Equals(Unwrap, o.Unwrap);
    }

    private void ValidateValue(object value)
    {
        var typesToValidate = new Stack<object>();
        typesToValidate.Push(value);

        while (typesToValidate.Count > 0)
        {
            var currentValue = typesToValidate.Pop();
            var currentType = currentValue.GetType();

            if (currentValue is IEnumerable && currentType != typeof(string))
            {
                foreach (var item in (IEnumerable)currentValue)
                {
                    if (item != null && !IsSimpleType(item.GetType()))
                    {
                        typesToValidate.Push(item);
                    }
                }
            }
            else if (IsDictionaryType(currentType))
            {
                ValidateDictionary(currentValue, typesToValidate);
            }
            else if (!IsSimpleType(currentType))
            {
                throw new ArgumentException($"Type '{currentType.Name}' is not supported.");
            }
        }
    }

    private void ValidateDictionary(object dictionaryValue, Stack<object> typesToValidate)
    {
        var dictionary = dictionaryValue as IDictionary;
        if (dictionary != null)
        {
            foreach (var key in dictionary.Keys)
            {
                if (key is not string)
                {
                    throw new ArgumentException("Keys of IDictionary must be of type string.");
                }
            }

            foreach (var value in dictionary.Values)
            {
                if (value != null && !IsSimpleType(value.GetType()))
                {
                    typesToValidate.Push(value);
                }
            }
        }
    }

    private bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || IsDateTimeType(type) || IsPoco(type);
    }

    private bool IsDateTimeType(Type type)
    {
        return type == typeof(DateTime) || type == typeof(DateTimeOffset);
    }

    private bool IsPoco(Type type)
    {
        bool isClassOrStruct = type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum);

        bool isUserDefined = !type.IsAbstract && (type.Namespace == null || !type.Namespace.StartsWith("System"));

        bool isNotQueryFragment = !typeof(IQueryFragment).IsAssignableFrom(type);

        return isClassOrStruct && isUserDefined && isNotQueryFragment;
    }

    private bool IsDictionaryType(Type type)
    {
        return type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }
}
