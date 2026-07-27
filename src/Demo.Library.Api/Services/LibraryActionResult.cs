using Demo.Library.Api.Endpoints.Me.Contracts;

namespace Demo.Library.Api.Services;

internal enum LibraryActionStatus
{
    Success,
    NotFound,
    Conflict
}

internal sealed record LibraryActionResult(
    LibraryActionStatus Status,
    string Message,
    BookTransactionResponse? Payload = null)
{
    public static LibraryActionResult Success(BookTransactionResponse payload)
        => new(LibraryActionStatus.Success, payload.Message, payload);

    public static LibraryActionResult NotFound(string message)
        => new(LibraryActionStatus.NotFound, message);

    public static LibraryActionResult Conflict(string message)
        => new(LibraryActionStatus.Conflict, message);
}