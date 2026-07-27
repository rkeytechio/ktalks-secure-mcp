using Demo.Library.Api.Endpoints.Me.Contracts;
using Demo.Library.Api.Endpoints.Search.Contracts;

namespace Demo.Library.Api.Services;

internal interface ILibraryService
{
    Task<IReadOnlyList<BookSearchResponse>> SearchBooksAsync(
        SearchBooksRequest request,
        CancellationToken cancellationToken = default);

    Task<LibraryActionResult> BorrowBookAsync(
        int bookId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<LibraryActionResult> ReturnBookAsync(
        int bookId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BorrowedBookResponse>> GetBorrowedBooksAsync(
        string userId,
        CancellationToken cancellationToken = default);
}