using Demo.Library.Api.Endpoints.Me.Contracts;
using Demo.Library.Api.Services;

namespace Demo.Library.Api.Endpoints.Me;

internal static class BorrowBookEndpoint
{
    public static RouteHandlerBuilder MapBorrowBook(this RouteGroupBuilder library)
    {
        return library.MapPost("/books/{bookId:int}/borrow", async (
            [AsParameters] BookRouteRequest request,
                HttpContext httpContext,
                ILibraryService libraryService,
                CancellationToken cancellationToken) =>
            {
                var userId = httpContext.GetCurrentUserId();
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var result = await libraryService.BorrowBookAsync(request.BookId, userId, cancellationToken);
                return result.Status switch
                {
                    LibraryActionStatus.NotFound => Results.NotFound(new { message = result.Message }),
                    LibraryActionStatus.Conflict => Results.Conflict(new { message = result.Message }),
                    _ => Results.Ok(result.Payload)
                };
            })
            .WithName("BorrowBook")
            .WithSummary("Borrow a book for the current user")
            .WithDescription("Requires a simulated authenticated user via the X-User-Id header.");
    }
}
