using Fauna.Serialization;
using Fauna.Types;
using Fauna.Util;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fauna.Linq;

internal interface IPipelineExecutor
{
    private static readonly MethodInfo _createEnumExec =
        typeof(IPipelineExecutor).GetMethod(nameof(CreateEnumExec), BindingFlags.Public | BindingFlags.Static)!;

    private static readonly MethodInfo _createScalarExec =
        typeof(IPipelineExecutor).GetMethod(nameof(CreateScalarExec), BindingFlags.Public | BindingFlags.Static)!;

    Type ElemType { get; }
    Type ResType { get; }

    IAsyncEnumerable<Page<object?>> PagedResult(QueryOptions? queryOptions, CancellationToken cancel = default);
    Task<object?> Result(QueryOptions? queryOptions, CancellationToken cancel = default);

    IAsyncEnumerable<Page<T>> PagedResult<T>(QueryOptions? queryOptions, CancellationToken cancel = default);
    Task<T> Result<T>(QueryOptions? queryOptions, CancellationToken cancel = default);

    public static IPipelineExecutor Create(
        DataContext ctx,
        Query query,
        ICodec deser,
        Delegate? proj,
        PipelineMode mode)
    {
        Debug.Assert(mode != PipelineMode.SetLoad);

        var innerTy = deser.GetType()
            .GetGenInst(typeof(ICodec<>))!
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

        return (IPipelineExecutor)exec!;
    }

    public static EnumExecutor<E> CreateEnumExec<I, E>(
        DataContext ctx,
        Query query,
        ICodec<I> deser,
        Func<I, E>? proj) =>
        new EnumExecutor<E>(ctx, query, new PageCodec<E>(MapDeser(deser, proj)));

    public static ScalarExecutor<E> CreateScalarExec<I, E>(
        DataContext ctx,
        Query query,
        ICodec<I> deser,
        Func<I, E>? proj) =>
        new ScalarExecutor<E>(ctx, query, MapDeser(deser, proj));

    private static ICodec<E> MapDeser<I, E>(ICodec<I> inner, Func<I, E>? proj)
    {
        if (proj is not null)
        {
            return new MappedCodec<I, E>(inner, proj);
        }

        Debug.Assert(typeof(I) == typeof(E));
        return (ICodec<E>)inner;
    }

    public readonly record struct EnumExecutor<E>(
        DataContext Ctx,
        Query Query,
        PageCodec<E> Deser) : IPipelineExecutor
    {
        public Type ElemType { get => typeof(E); }
        public Type ResType { get => typeof(IEnumerable<E>); }

        public IAsyncEnumerable<Page<T>> PagedResult<T>(QueryOptions? queryOptions, CancellationToken cancel = default)
        {
            var pages = Ctx.PaginateAsyncInternal(Query, Deser, queryOptions, cancel);
            if (pages is IAsyncEnumerable<Page<T>> ret)
            {
                return ret;
            }

            Debug.Assert(typeof(T) == ElemType);
            throw new Exception("unreachable");
        }

        public async Task<T> Result<T>(QueryOptions? queryOptions, CancellationToken cancel = default)
        {
            var pages = PagedResult<E>(queryOptions, cancel);
            var elems = new List<E>();

            if (elems is T res)
            {
                await foreach (var page in pages)
                {
                    cancel.ThrowIfCancellationRequested();
                    elems.AddRange(page.Data);
                }

                return res;
            }

            Debug.Assert(typeof(T) == ResType, $"{typeof(T)} is not {ResType}");
            throw new Exception("unreachable");
        }

        public async IAsyncEnumerable<Page<object?>> PagedResult(QueryOptions? queryOptions, [EnumeratorCancellation] CancellationToken cancel = default)
        {
            await foreach (var page in PagedResult<E>(queryOptions, cancel))
            {
                var data = page.Data.Select(e => (object?)e).ToList();
                yield return new Page<object?>(data, page.After);
            }
        }

        public async Task<object?> Result(QueryOptions? queryOptions, CancellationToken cancel = default) =>
            await Result<IEnumerable<E>>(queryOptions, cancel);
    }


    public readonly record struct ScalarExecutor<E>(
        DataContext Ctx,
        Query Query,
        ICodec<E> Deser) : IPipelineExecutor
    {
        public Type ElemType { get => typeof(E); }
        public Type ResType { get => typeof(E); }

        public async Task<T> Result<T>(QueryOptions? queryOptions, CancellationToken cancel = default)
        {
            var qres = await Ctx.QueryAsync(Query, Deser, queryOptions, cancel);
            if (qres.Data is T ret)
            {
                return ret;
            }

            if (qres.Data is null)
            {
                return default(T)!;
            }

            Debug.Assert(typeof(T) == ResType, $"{typeof(T)} is not {ResType}");
            throw new Exception("unreachable");
        }

        public async IAsyncEnumerable<Page<T>> PagedResult<T>(QueryOptions? queryOptions, [EnumeratorCancellation] CancellationToken cancel = default)
        {
            if (await Result<E>(queryOptions, cancel) is T ret)
            {
                yield return new Page<T>(new List<T> { ret }, null);
            }

            Debug.Assert(typeof(T) == ElemType);
            throw new Exception("unreachable");
        }

        public async Task<object?> Result(QueryOptions? queryOptions, CancellationToken cancel = default) =>
            await Result<E>(queryOptions, cancel);

        public async IAsyncEnumerable<Page<object?>> PagedResult(QueryOptions? queryOptions, [EnumeratorCancellation] CancellationToken cancel = default)
        {
            yield return new Page<object?>(new List<object?> { await Result(queryOptions, cancel) }, null);
        }
    }
}
