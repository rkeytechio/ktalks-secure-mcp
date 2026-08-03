using Newtonsoft.Json;

namespace Demo.Library.Api.Persistence.Entities;

internal sealed record class EndpointActivityLogEntity
{
    public const string WebActivityType = "Web";

    [JsonProperty(PropertyName = "id")]
    public string Id { get; init; } = Guid.CreateVersion7().ToString("n");

    [JsonProperty(PropertyName = "activitytype")]
    public string ActivityType { get; init; } = WebActivityType;

    public string PartitionKey => ActivityType;

    [JsonProperty(PropertyName = "timestamputc")]
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    [JsonProperty(PropertyName = "correlationid")]
    public string? CorrelationId { get; init; }

    [JsonProperty(PropertyName = "method")]
    public string? Method { get; init; }

    [JsonProperty(PropertyName = "path")]
    public string? Path { get; init; }

    [JsonProperty(PropertyName = "querystring")]
    public string? QueryString { get; init; }

    [JsonProperty(PropertyName = "statuscode")]
    public int StatusCode { get; init; }

    [JsonProperty(PropertyName = "durationms")]
    public long DurationMs { get; init; }

    [JsonProperty(PropertyName = "userid")]
    public string? UserId { get; init; }

    [JsonProperty(PropertyName = "clientip")]
    public string? ClientIp { get; init; }

    [JsonProperty(PropertyName = "useragent")]
    public string? UserAgent { get; init; }

    [JsonProperty(PropertyName = "requestcontenttype")]
    public string? RequestContentType { get; init; }

    [JsonProperty(PropertyName = "requestbody")]
    public string? RequestBody { get; init; }

    [JsonProperty(PropertyName = "requestbodywastruncated")]
    public bool RequestBodyWasTruncated { get; init; }

    [JsonProperty(PropertyName = "responsecontenttype")]
    public string? ResponseContentType { get; init; }

    [JsonProperty(PropertyName = "responsebody")]
    public string? ResponseBody { get; init; }

    [JsonProperty(PropertyName = "responsebodywastruncated")]
    public bool ResponseBodyWasTruncated { get; init; }

    [JsonProperty(PropertyName = "requestheaders")]
    public Dictionary<string, string> RequestHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonProperty(PropertyName = "responseheaders")]
    public Dictionary<string, string> ResponseHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonProperty(PropertyName = "errormessage")]
    public string? ErrorMessage { get; init; }
}
