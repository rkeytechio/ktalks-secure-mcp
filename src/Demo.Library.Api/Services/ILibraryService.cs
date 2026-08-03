using Demo.Library.Api.Endpoints.Search.Contracts;

namespace Demo.Library.Api.Services;

internal interface ILibraryService
{
    Task<IReadOnlyList<BookSearchResponse>> SearchBooksAsync(
        SearchBooksRequest request,
        CancellationToken cancellationToken = default);

    Task<LibraryActionResult> BorrowBookAsync(
        string bookId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<LibraryActionResult> ReturnBookAsync(
        string bookId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<LibraryActionResult> GetBorrowedBooksAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<LibraryActionResult> RequestAccountClosureAsync(
        string userId,
        string reason,
        CancellationToken cancellationToken = default);
}