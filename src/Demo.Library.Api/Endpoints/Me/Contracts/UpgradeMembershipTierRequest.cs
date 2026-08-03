namespace Demo.Library.Api.Endpoints.Me.Contracts;

internal sealed record UpgradeMembershipTierRequest(
    string TargetTier);