using System.Collections.ObjectModel;

namespace Fauna;

public abstract partial class Query
{
    public sealed class Expr : Query
    {
        public Expr(IList<object> fragments)
        {
            ValidateFragments(fragments);
            Fragments = new ReadOnlyCollection<object>(fragments);
        }

        public Expr(params object[] fragments)
        {
            ValidateFragments(fragments);
            Fragments = new ReadOnlyCollection<object>(fragments.ToList());
        }

        public ReadOnlyCollection<object> Fragments { get; }

        private void ValidateFragments(IEnumerable<object> fragments)
        {
            if (fragments == null)
            {
                throw new ArgumentNullException(nameof(fragments));
            }

            foreach (var fragment in fragments)
            {
                //TODO: Add Query.Obj And Query.Arr
                Type fragmentType = fragment.GetType();
                if (!(fragment is string || fragment is Expr || fragmentType.IsGenericType && fragmentType.GetGenericTypeDefinition() == typeof(Val<>)))
                {
                    throw new ArgumentException($"Invalid fragment type: {fragment.GetType()}");
                }
            }
        }

        public override bool Equals(Query? o) => IsEqual(o as Expr);

        public override bool Equals(object? o)
        {
            if (ReferenceEquals(this, o))
            {
                return true;
            }

            return o is Expr expr && IsEqual(expr);
        }

        public override int GetHashCode() => Fragments.GetHashCode();

        public override string ToString() => $"Expr({string.Join(",", Fragments)})";

        private bool IsEqual(Expr? o)
        {
            if (o is null)
            {
                return false;
            }

            if (Fragments == null || o.Fragments == null)
            {
                return Fragments == null && o.Fragments == null;
            }

            return Fragments.SequenceEqual(o.Fragments);
        }

        public static bool operator ==(Expr left, Expr right)
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

        public static bool operator !=(Expr left, Expr right)
        {
            return !(left == right);
        }
    }
}
