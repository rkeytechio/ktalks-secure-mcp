namespace Demo.Library.Api.Endpoints.Me.Contracts;

internal sealed record BookTransactionResponse(
    string Message,
    int Id,
    string Title,
    int AvailableCopies);
