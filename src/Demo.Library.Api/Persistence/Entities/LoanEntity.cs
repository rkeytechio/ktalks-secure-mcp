using Newtonsoft.Json;

namespace Demo.Library.Api.Persistence.Entities;

internal sealed record class LoanEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty(PropertyName = "bookid")]
    public string BookId { get; set; } = string.Empty;

    public BookEntity? Book { get; set; }

    [JsonProperty(PropertyName = "userid")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "borrowedatutc")]
    public DateTime BorrowedAtUtc { get; set; }

    [JsonProperty(PropertyName = "returnedatutc")]
    public DateTime? ReturnedAtUtc { get; set; }
}
