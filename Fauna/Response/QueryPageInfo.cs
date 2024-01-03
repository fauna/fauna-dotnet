using System.Text.Json.Serialization;

namespace Fauna.Response
{
    public readonly struct QueryPageInfo
    {
        [JsonPropertyName("data")]
        public object Data { get; init; }

        [JsonPropertyName("after")]
        public string After { get; init; }
    }
}
