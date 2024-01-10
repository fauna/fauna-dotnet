namespace Fauna;

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
    /// Format is a list of key:value pairs, each key and value limited to [a-zA-Z0-9_].
    /// </summary>
    public Dictionary<string, string>? QueryTags { get; set; } = null;

    /// <summary>
    /// Gets or sets the trace parent identifier for distributed tracing systems.
    /// </summary>
    public string? TraceParent { get; set; } = null;

    /// <summary>
    /// Combines default query options with overrides from the client configuration.
    /// </summary>
    /// <param name="defaultQueryOptions">The default query options.</param>
    /// <param name="queryOptionOverrides">The query options provided for a specific query, overriding the defaults.</param>
    /// <returns>A <see cref="QueryOptions"/> object representing the final combined set of query options.</returns>
    internal static QueryOptions? GetFinalQueryOptions(QueryOptions? defaultQueryOptions, QueryOptions? queryOptionOverrides)
    {

        if (defaultQueryOptions == null && queryOptionOverrides == null)
        {
            return null;
        }
        else if (defaultQueryOptions == null)
        {
            return queryOptionOverrides;
        }
        else if (queryOptionOverrides == null)
        {
            return defaultQueryOptions;
        }

        var finalQueryOptions = new QueryOptions()
        {
            Linearized = defaultQueryOptions.Linearized,
            TypeCheck = defaultQueryOptions.TypeCheck,
            QueryTimeout = defaultQueryOptions.QueryTimeout,
            QueryTags = defaultQueryOptions.QueryTags,
            TraceParent = defaultQueryOptions.TraceParent,
        };

        var properties = typeof(QueryOptions).GetProperties();

        foreach (var prop in properties)
        {
            if (prop.Name.Equals(nameof(QueryTags)))
            {
                continue;
            }

            var propertyOverride = prop.GetValue(queryOptionOverrides);

            if (propertyOverride != null)
            {
                prop.SetValue(finalQueryOptions, propertyOverride);
            }
        }

        if (queryOptionOverrides.QueryTags != null)
        {
            if (finalQueryOptions.QueryTags == null)
            {
                finalQueryOptions.QueryTags = queryOptionOverrides.QueryTags;
            }
            else
            {
                foreach (var kv in queryOptionOverrides.QueryTags)
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
