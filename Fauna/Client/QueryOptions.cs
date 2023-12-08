namespace Fauna;

public class QueryOptions
{
    public bool? Linearized { get; set; } = null;
    public bool? TypeCheck { get; set; } = null;
    public TimeSpan? QueryTimeout { get; set; } = null;
    public Dictionary<string, string>? QueryTags { get; set; } = null;
    public string? TraceParent { get; set; } = null;

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
