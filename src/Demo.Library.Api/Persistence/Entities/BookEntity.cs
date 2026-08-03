using Newtonsoft.Json;

namespace Demo.Library.Api.Persistence.Entities;

internal sealed record class BookEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty(PropertyName = "isbn")]
    public string Isbn { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "author")]
    public string Author { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "totalcopies")]
    public int TotalCopies { get; set; }

    [JsonProperty(PropertyName = "availablecopies")]
    public int AvailableCopies { get; set; }
}
