using System.Text.Json.Nodes;

namespace Fauna.Test.Helpers
{
    public static class ExceptionTestHelper
    {
        public static QueryFailure CreateQueryFailure(string code, string? message = null, string? abortData = null)
        {
            var errorObject = new JsonObject
            {
                ["code"] = JsonValue.Create(code),
                ["message"] = JsonValue.Create(message ?? "Error message")
            };

            if (!string.IsNullOrWhiteSpace(abortData))
            {
                errorObject["abort"] = JsonValue.Create(abortData);
            }

            var responseObject = new JsonObject
            {
                ["error"] = errorObject,
                ["summary"] = JsonValue.Create("error: Query aborted."),
                ["txn_ts"] = JsonValue.Create(1702346199930000),
                ["stats"] = CreateDefaultStats(),
                ["schema_version"] = JsonValue.Create(0)
            };

            string rawResponseText = responseObject.ToString();
            return new QueryFailure(rawResponseText);
        }

        private static JsonObject CreateDefaultStats()
        {
            return new JsonObject
            {
                ["compute_ops"] = JsonValue.Create(1),
                ["read_ops"] = JsonValue.Create(0),
                ["write_ops"] = JsonValue.Create(0),
                ["query_time_ms"] = JsonValue.Create(105),
                ["contention_retries"] = JsonValue.Create(0),
                ["storage_bytes_read"] = JsonValue.Create(261),
                ["storage_bytes_write"] = JsonValue.Create(0),
                ["rate_limits_hit"] = new JsonArray()
            };
        }
    }
}
