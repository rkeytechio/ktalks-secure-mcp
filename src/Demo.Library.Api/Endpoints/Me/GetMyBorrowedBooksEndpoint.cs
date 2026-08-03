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

                var result = await libraryService.GetBorrowedBooksAsync(userId, cancellationToken);
                return result.Status switch
                {
                    LibraryActionStatus.Conflict => Results.Conflict(new { message = result.Message }),
                    _ => Results.Ok(result.Payload)
                };
            })
            .WithName("GetMyBorrowedBooks")
            .WithSummary("List books currently in the user's possession")
            .WithDescription("An authenticated active user can view their currently borrowed books.");
    }
}
