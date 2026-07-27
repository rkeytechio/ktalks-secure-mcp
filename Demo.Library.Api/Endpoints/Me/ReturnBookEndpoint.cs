using Demo.Library.Api.Endpoints.Me.Contracts;
using Demo.Library.Api.Services;

namespace Demo.Library.Api.Endpoints.Me;

internal static class ReturnBookEndpoint
{
    public static RouteHandlerBuilder MapReturnBook(this RouteGroupBuilder library)
    {
        return library.MapPost("/books/{bookId:int}/return", async (
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

                var result = await libraryService.ReturnBookAsync(request.BookId, userId, cancellationToken);
                return result.Status switch
                {
                    LibraryActionStatus.NotFound => Results.NotFound(new { message = result.Message }),
                    LibraryActionStatus.Conflict => Results.Conflict(new { message = result.Message }),
                    _ => Results.Ok(result.Payload)
                };
            })
            .WithName("ReturnBook")
            .WithSummary("Return a borrowed book for the current user")
            .WithDescription("Requires a simulated authenticated user via the X-User-Id header.");
    }
}
