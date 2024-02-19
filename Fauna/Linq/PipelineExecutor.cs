using Fauna.Serialization;
using Fauna.Types;
using Fauna.Util;
using System.Diagnostics;
using System.Reflection;

namespace Fauna.Linq;

internal interface PipelineExecutor
{
    private static readonly MethodInfo _createEnumExec =
        typeof(PipelineExecutor).GetMethod(nameof(CreateEnumExec), BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo _createScalarExec =
        typeof(PipelineExecutor).GetMethod(nameof(CreateScalarExec), BindingFlags.Public | BindingFlags.Static)!;

    Type ElemType { get; }
    Type ResType { get; }

    IAsyncEnumerable<Page<object?>> PagedResult(QueryOptions? queryOptions);
    Task<object?> Result(QueryOptions? queryOptions);

    IAsyncEnumerable<Page<T>> PagedResult<T>(QueryOptions? queryOptions);
    Task<T> Result<T>(QueryOptions? queryOptions);

    public static PipelineExecutor Create(
        DataContext ctx,
        Query query,
        IDeserializer deser,
        Delegate? proj,
        PipelineMode mode)
    {
        Debug.Assert(mode != PipelineMode.SetLoad);

        var innerTy = deser.GetType()
            .GetGenInst(typeof(IDeserializer<>))!
            .GetGenericArguments()[0];

        var elemTy = proj is null ?
            innerTy :
            proj.GetType().GetGenInst(typeof(Func<,>))!
            .GetGenericArguments()[1];

        var method = mode switch
        {
            PipelineMode.Query or PipelineMode.Project => _createEnumExec,
            PipelineMode.Scalar => _createScalarExec,
            _ => throw new Exception("unreachable"),
        };

        var typeArgs = new Type[] { innerTy, elemTy };
        var args = new object?[] { ctx, query, deser, proj };
        var exec = method.MakeGenericMethod(typeArgs).Invoke(null, args);

        return (PipelineExecutor)exec!;
    }

    public static EnumExecutor<E> CreateEnumExec<I, E>(
        DataContext ctx,
        Query query,
        IDeserializer<I> deser,
        Func<I, E>? proj) =>
        new EnumExecutor<E>(ctx, query, new PageDeserializer<E>(MapDeser(deser, proj)));

    public static ScalarExecutor<E> CreateScalarExec<I, E>(
        DataContext ctx,
        Query query,
        IDeserializer<I> deser,
        Func<I, E>? proj) =>
        new ScalarExecutor<E>(ctx, query, MapDeser(deser, proj));

    private static IDeserializer<E> MapDeser<I, E>(IDeserializer<I> inner, Func<I, E>? proj)
    {
        if (proj is not null)
        {
            return new MappedDeserializer<I, E>(inner, proj);
        }

        Debug.Assert(typeof(I) == typeof(E));
        return (IDeserializer<E>)inner;
    }

    public readonly record struct EnumExecutor<E>(
        DataContext Ctx,
        Query Query,
        PageDeserializer<E> Deser) : PipelineExecutor
    {
        public Type ElemType { get => typeof(E); }
        public Type ResType { get => typeof(IEnumerable<E>); }

        public IAsyncEnumerable<Page<T>> PagedResult<T>(QueryOptions? queryOptions)
        {
            var pages = Ctx.PaginateAsyncInternal(Query, Deser, queryOptions);
            if (pages is IAsyncEnumerable<Page<T>> ret)
            {
                return ret;
            }

            Debug.Assert(typeof(T) == ElemType);
            throw new Exception("unreachable");
        }

        public async Task<T> Result<T>(QueryOptions? queryOptions)
        {
            var pages = PagedResult<E>(queryOptions);
            var elems = new List<E>();

            if (elems is T res)
            {
                await foreach (var page in pages)
                {
                    elems.AddRange(page.Data);
                }

                return res;
            }

            Debug.Assert(typeof(T) == ResType, $"{typeof(T)} is not {ResType}");
            throw new Exception("unreachable");
        }

        public async IAsyncEnumerable<Page<object?>> PagedResult(QueryOptions? queryOptions)
        {
            await foreach (var page in PagedResult<E>(queryOptions))
            {
                var data = page.Data.Select(e => (object?)e).ToList();
                yield return new Page<object?>(data, page.After);
            }
        }

        public async Task<object?> Result(QueryOptions? queryOptions) =>
            await Result<IEnumerable<E>>(queryOptions);
    }


    public readonly record struct ScalarExecutor<E>(
        DataContext Ctx,
        Query Query,
        IDeserializer<E> Deser) : PipelineExecutor
    {
        public Type ElemType { get => typeof(E); }
        public Type ResType { get => typeof(E); }

        public async Task<T> Result<T>(QueryOptions? queryOptions)
        {
            var qres = await Ctx.QueryAsync(Query, Deser, queryOptions);
            if (qres.Data is T ret)
            {
                return ret;
            }

            Debug.Assert(typeof(T) == ResType, $"{typeof(T)} is not {ResType}");
            throw new Exception("unreachable");
        }

        public async IAsyncEnumerable<Page<T>> PagedResult<T>(QueryOptions? queryOptions)
        {
            if (await Result<E>(queryOptions) is T ret)
            {
                yield return new Page<T>(new List<T> { ret }, null);
            }

            Debug.Assert(typeof(T) == ElemType);
            throw new Exception("unreachable");
        }

        public async Task<object?> Result(QueryOptions? queryOptions) =>
            await Result<E>(queryOptions);

        public async IAsyncEnumerable<Page<object?>> PagedResult(QueryOptions? queryOptions)
        {
            yield return new Page<object?>(new List<object?> { await Result(queryOptions) }, null);
        }
    }
}
