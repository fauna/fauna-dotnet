namespace Fauna;

// FIXME(matt) swap in actual value type
using Value = Object;

public abstract class Query : IEquatable<Query>
{
    private Query() {}

    public abstract bool Equals(Query? o);
    public override bool Equals(object? o) => Equals(o as Query);

    public sealed class Val : Query
    {
        public Val(Value v)
        {
            Unwrap = v;
        }

        public Value Unwrap { get; }

        public override bool Equals(Query? o) => IsEqual(o as Val);
        public override int GetHashCode() => Unwrap.GetHashCode();

        public override string ToString() => $"Val({Unwrap})";

        private bool IsEqual(Val? o) => o != null && Unwrap == o.Unwrap;
    }

    public sealed class Expr : Query
    {
        public Expr(List<object> fragments)
        {
            Fragments = fragments;
        }

        public Expr(params object[] fragments)
        {
            Fragments = fragments.ToList();
        }

        public List<Object> Fragments { get; }

        public override bool Equals(Query? o) => IsEqual(o as Expr);
        public override int GetHashCode() => Fragments.GetHashCode();

        public override string ToString() => $"Expr({string.Join(",", Fragments)})";

        private bool IsEqual(Expr? o) => o != null && Fragments.SequenceEqual(o.Fragments);
    }

    // template-based construction

    public static Query FQL(ref QueryStringHandler handler)
    {
        return handler.Result();
    }
}
