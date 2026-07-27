namespace Demo.Library.Api.Endpoints.Me.Contracts;

internal sealed record BorrowedBookResponse(
    int BookId,
    string Isbn,
    string Title,
    string Author,
    DateTime BorrowedAtUtc);
