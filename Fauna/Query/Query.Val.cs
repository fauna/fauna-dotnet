namespace Fauna;

public abstract partial class Query
{
    public sealed class Val<T> : Query
    {
        public Val(T v)
        {
            if (v == null)
            {
                throw new ArgumentNullException(nameof(v), "Value cannot be null.");
            }

            Unwrap = v;
        }

        public T Unwrap { get; }

        public override bool Equals(Query? o) => IsEqual(o as Val<T>);

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

            return IsEqual(o as Val<T>);
        }

        public override int GetHashCode() => Unwrap != null ? EqualityComparer<T>.Default.GetHashCode(Unwrap) : 0;

        public override string ToString() => $"Val({Unwrap})";

        public static bool operator ==(Val<T> left, Val<T> right)
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

        public static bool operator !=(Val<T> left, Val<T> right)
        {
            return !(left == right);
        }

        private bool IsEqual(Val<T>? o)
        {
            if (o is null)
            {
                return false;
            }

            return EqualityComparer<T>.Default.Equals(Unwrap, o.Unwrap);
        }
    }
}
