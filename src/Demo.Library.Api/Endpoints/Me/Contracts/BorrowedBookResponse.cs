namespace Demo.Library.Api.Endpoints.Me.Contracts;

internal sealed record BorrowedBookResponse(
    string BookId,
    string Isbn,
    string Title,
    string Author,
    DateTime BorrowedAtUtc);
