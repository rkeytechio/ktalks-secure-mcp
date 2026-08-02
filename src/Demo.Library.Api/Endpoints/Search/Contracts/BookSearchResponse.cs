namespace Demo.Library.Api.Endpoints.Search.Contracts;

internal sealed record BookSearchResponse(
    string Id,
    string Isbn,
    string Title,
    string Author,
    int AvailableCopies,
    int TotalCopies);
