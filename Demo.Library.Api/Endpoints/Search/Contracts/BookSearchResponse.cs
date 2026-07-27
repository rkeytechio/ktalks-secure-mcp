namespace Demo.Library.Api.Endpoints.Search.Contracts;

internal sealed record BookSearchResponse(
    int Id,
    string Isbn,
    string Title,
    string Author,
    int AvailableCopies,
    int TotalCopies);
