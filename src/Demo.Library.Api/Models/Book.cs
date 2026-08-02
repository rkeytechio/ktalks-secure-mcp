namespace Demo.Library.Api.Models;

internal sealed class Book
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Isbn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}
