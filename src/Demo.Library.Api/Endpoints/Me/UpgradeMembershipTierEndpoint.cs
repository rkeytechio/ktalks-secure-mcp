using Demo.Library.Api.Endpoints.Me.Contracts;

namespace Demo.Library.Api.Endpoints.Me;

internal static class UpgradeMembershipTierEndpoint
{
    public static RouteHandlerBuilder MapUpgradeMembershipTier(this RouteGroupBuilder library)
    {
        return library.MapPost("/membership-tier/upgrade", (
                UpgradeMembershipTierRequest request,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetCurrentUserId();
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var response = new UpgradeMembershipTierResponse(
                    $"Membership tier upgrade endpoint is currently a stub. Requested tier: {request.TargetTier}");

                return Results.Ok(response);
            })
            .WithName("UpgradeMembershipTier")
            .WithSummary("Request a membership tier upgrade for the current user")
            .WithDescription("An authenticated active user can submit a membership tier upgrade request.");
    }
}