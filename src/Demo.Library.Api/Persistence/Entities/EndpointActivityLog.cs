using Newtonsoft.Json;
using Demo.Library.Api.Persistence.Abstractions;

namespace Demo.Library.Api.Persistence.Entities;

internal sealed class EndpointActivityLog
    : ICosmosEntity
{
    public const string WebActivityType = "Web";

    [JsonProperty(PropertyName = "id")]
    public string Id { get; init; } = Guid.CreateVersion7().ToString("n");

    public string ActivityType { get; init; } = WebActivityType;

    public string PartitionKey => ActivityType;

    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    public string? CorrelationId { get; init; }

    public string? Method { get; init; }

    public string? Path { get; init; }

    public string? QueryString { get; init; }

    public int StatusCode { get; init; }

    public long DurationMs { get; init; }

    public string? UserId { get; init; }

    public string? ClientIp { get; init; }

    public string? UserAgent { get; init; }

    public string? RequestContentType { get; init; }

    public string? RequestBody { get; init; }

    public bool RequestBodyWasTruncated { get; init; }

    public string? ResponseContentType { get; init; }

    public string? ResponseBody { get; init; }

    public bool ResponseBodyWasTruncated { get; init; }

    public Dictionary<string, string> RequestHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> ResponseHeaders { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public string? ErrorMessage { get; init; }
}