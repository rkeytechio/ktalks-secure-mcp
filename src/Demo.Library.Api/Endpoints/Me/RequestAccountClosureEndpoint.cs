using Demo.Library.Api.Endpoints.Me.Contracts;
using Demo.Library.Api.Services;

namespace Demo.Library.Api.Endpoints.Me;

internal static class RequestAccountClosureEndpoint
{
    public static RouteHandlerBuilder MapRequestAccountClosure(this RouteGroupBuilder library)
    {
        return library.MapPost("/close-account", async (
                AccountClosureRequest request,
                HttpContext httpContext,
                ILibraryService libraryService,
                CancellationToken cancellationToken) =>
            {
                var userId = httpContext.GetCurrentUserId();
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return Results.BadRequest(new { message = "A reason for closing the account is required." });
                }

                var result = await libraryService.RequestAccountClosureAsync(
                    userId,
                    request.Reason.Trim(),
                    cancellationToken);

                return result.Status switch
                {
                    LibraryActionStatus.Conflict => Results.Conflict(new { message = result.Message }),
                    _ => Results.Accepted(value: new { message = result.Message })
                };
            })
            .WithName("RequestAccountClosure")
            .WithSummary("Request closure of the current user's account")
            .WithDescription("The current user must have no borrowed books. Requires a simulated authenticated user via the X-User-Id header.");
    }
}