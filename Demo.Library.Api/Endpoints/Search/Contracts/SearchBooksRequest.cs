using Microsoft.AspNetCore.Mvc;

namespace Demo.Library.Api.Endpoints.Search.Contracts;

internal sealed record SearchBooksRequest(
    [property: FromQuery(Name = "query")] string? Query,
    string? Author,
    string? Isbn,
    bool AvailableOnly = false);
