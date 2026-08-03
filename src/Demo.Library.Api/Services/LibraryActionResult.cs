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
    object? Payload = null)
{
    public static LibraryActionResult Success(object payload, string message = "")
        => new(LibraryActionStatus.Success, message, payload);

    public static LibraryActionResult Success(string message)
        => new(LibraryActionStatus.Success, message);

    public static LibraryActionResult NotFound(string message)
        => new(LibraryActionStatus.NotFound, message);

    public static LibraryActionResult Conflict(string message)
        => new(LibraryActionStatus.Conflict, message);
}