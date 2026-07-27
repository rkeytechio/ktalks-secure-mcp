using Demo.Library.Api.Endpoints.Search.Contracts;
using Demo.Library.Api.Services;

namespace Demo.Library.Api.Endpoints.Search;

internal static class SearchBooksEndpoint
{
    public static RouteHandlerBuilder MapSearchBooks(this RouteGroupBuilder library)
    {
        return library.MapGet("/books", async (
                [AsParameters] SearchBooksRequest request,
                ILibraryService libraryService,
                CancellationToken cancellationToken) =>
            {
                var books = await libraryService.SearchBooksAsync(request, cancellationToken);

                return Results.Ok(books);
            })
            .WithName("SearchBooks")
            .WithSummary("Search books in the library catalog");
    }
}
