namespace Demo.Library.Api.Persistence.Entities;

internal sealed record class LoanEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BookId { get; set; } = string.Empty;
    public BookEntity? Book { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime BorrowedAtUtc { get; set; }
    public DateTime? ReturnedAtUtc { get; set; }
}
