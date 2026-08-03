namespace Demo.Library.Api.Persistence.Entities;

internal sealed record class BookEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Isbn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}
