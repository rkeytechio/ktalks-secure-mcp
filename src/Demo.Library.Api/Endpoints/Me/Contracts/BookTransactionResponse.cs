namespace Demo.Library.Api.Endpoints.Me.Contracts;

internal sealed record BookTransactionResponse(
    string Message,
    string Id,
    string Title,
    int AvailableCopies);
