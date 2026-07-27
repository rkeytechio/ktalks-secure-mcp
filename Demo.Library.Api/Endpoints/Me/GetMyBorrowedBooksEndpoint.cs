using Demo.Library.Api.Services;

namespace Demo.Library.Api.Endpoints.Me;

internal static class GetMyBorrowedBooksEndpoint
{
    public static RouteHandlerBuilder MapGetMyBorrowedBooks(this RouteGroupBuilder library)
    {
        return library.MapGet("/books", async (
                HttpContext httpContext,
                ILibraryService libraryService,
                CancellationToken cancellationToken) =>
            {
                var userId = httpContext.GetCurrentUserId();
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var borrowedBooks = await libraryService.GetBorrowedBooksAsync(userId, cancellationToken);
                return Results.Ok(borrowedBooks);
            })
            .WithName("GetMyBorrowedBooks")
            .WithSummary("List books currently in the user's possession")
            .WithDescription("Requires a simulated authenticated user via the X-User-Id header.");
    }
}
