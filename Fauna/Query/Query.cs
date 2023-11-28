namespace Fauna;

// FIXME(matt) swap in actual value type
using Value = Object;

public abstract class Query {
    private Query() {}

    public sealed class Val : Query
    {
        public Val(Value v)
        {
            Unwrap = v;
        }

        public Value Unwrap { get; }
    }

    public sealed class Expr : Query
    {
        public Expr(List<Object> fragments)
        {
            Fragments = fragments;
        }

        public List<Object> Fragments { get; }
    }

    // template-based construction

    public static Query FQL(ref QueryStringHandler handler)
    {
        return handler.Result();
    }
}
