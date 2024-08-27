namespace Fauna.Protocol;

/// <summary>
/// Represents the options for customizing Fauna queries.
/// </summary>
public class QueryOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the query runs as strictly serialized, affecting read-only transactions.
    /// </summary>
    public bool? Linearized { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether type checking of the query is enabled or disabled before evaluation.
    /// </summary>
    public bool? TypeCheck { get; set; } = null;

    /// <summary>
    /// Gets or sets the query timeout. It defines how long the client waits for a query to complete.
    /// </summary>
    public TimeSpan? QueryTimeout { get; set; } = null;

    /// <summary>
    /// Gets or sets a string-encoded set of caller-defined tags for identifying the request in logs and response bodies. 
    /// Each key and value should be limited to [a-zA-Z0-9_].
    /// </summary>
    public Dictionary<string, string>? QueryTags { get; set; } = null;

    /// <summary>
    /// Gets or sets the trace parent identifier for distributed tracing systems.
    /// </summary>
    public string? TraceParent { get; set; } = null;

    /// <summary>
    /// Merges two instances of <see cref="QueryOptions"/>.
    /// </summary>
    /// <param name="options">The default query options.</param>
    /// <param name="overrides">The query options provided for a specific query, overriding the defaults.</param>
    /// <returns>A <see cref="QueryOptions"/> object representing the final combined set of query options.</returns>
    internal static QueryOptions? GetFinalQueryOptions(QueryOptions? options, QueryOptions? overrides)
    {

        if (options == null && overrides == null)
        {
            return null;
        }

        if (options == null)
        {
            return overrides;
        }

        if (overrides == null)
        {
            return options;
        }

        var finalQueryOptions = new QueryOptions()
        {
            Linearized = options.Linearized,
            TypeCheck = options.TypeCheck,
            QueryTimeout = options.QueryTimeout,
            QueryTags = options.QueryTags,
            TraceParent = options.TraceParent,
        };

        var properties = typeof(QueryOptions).GetProperties();

        foreach (var prop in properties)
        {
            if (prop.Name.Equals(nameof(QueryTags)))
            {
                continue;
            }

            var propertyOverride = prop.GetValue(overrides);

            if (propertyOverride != null)
            {
                prop.SetValue(finalQueryOptions, propertyOverride);
            }
        }

        if (overrides.QueryTags != null)
        {
            if (finalQueryOptions.QueryTags == null)
            {
                finalQueryOptions.QueryTags = overrides.QueryTags;
            }
            else
            {
                foreach (var kv in overrides.QueryTags)
                {
                    if (finalQueryOptions.QueryTags.ContainsKey(kv.Key))
                    {
                        finalQueryOptions.QueryTags[kv.Key] = kv.Value;
                    }
                    else
                    {
                        finalQueryOptions.QueryTags.Add(kv.Key, kv.Value);
                    }
                }
            }
        }

        return finalQueryOptions;
    }
}
