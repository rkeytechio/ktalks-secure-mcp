namespace Demo.Library.Api.Models;

internal sealed class Loan
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BookId { get; set; } = string.Empty;
    public Book? Book { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime BorrowedAtUtc { get; set; }
    public DateTime? ReturnedAtUtc { get; set; }
}
