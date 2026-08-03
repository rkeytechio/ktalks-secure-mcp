namespace Demo.Library.Api.Persistence.Entities;

internal enum AccountClosureRequestStatus
{
    Pending,
    Cancelled
}

internal sealed record class AccountClosureRequestEntity
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime RequestedAtUtc { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AccountClosureRequestStatus Status { get; set; } = AccountClosureRequestStatus.Pending;
}