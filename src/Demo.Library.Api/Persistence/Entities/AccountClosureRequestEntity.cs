using Newtonsoft.Json;

namespace Demo.Library.Api.Persistence.Entities;

internal enum AccountClosureRequestStatus
{
    Pending,
    Cancelled
}

internal sealed record class AccountClosureRequestEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "userid")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "requestedatutc")]
    public DateTime RequestedAtUtc { get; set; }

    [JsonProperty(PropertyName = "reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "status")]
    public AccountClosureRequestStatus Status { get; set; } = AccountClosureRequestStatus.Pending;
}