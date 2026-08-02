using Demo.Library.Api.Endpoints.Me.Contracts;
using Demo.Library.Api.Endpoints.Search.Contracts;
using Demo.Library.Api.Models;
using Demo.Library.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Demo.Library.Api.Services;

internal sealed class LibraryService(LibraryDbContext db) : ILibraryService
{
    public async Task<IReadOnlyList<BookSearchResponse>> SearchBooksAsync(
        SearchBooksRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = db.Books.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = request.Query.Trim().ToLowerInvariant();
            query = query.Where(b =>
                MatchesLikePattern(b.Title.ToLower(), term) ||
                MatchesLikePattern(b.Author.ToLower(), term));
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            var authorTerm = request.Author.Trim().ToLowerInvariant();
            query = query.Where(b => MatchesLikePattern(b.Author.ToLower(), authorTerm));
        }

        if (!string.IsNullOrWhiteSpace(request.Isbn))
        {
            var normalizedIsbn = request.Isbn.Trim();
            query = query.Where(b => b.Isbn == normalizedIsbn);
        }

        if (request.AvailableOnly)
        {
            query = query.Where(b => b.AvailableCopies > 0);
        }

        return await query
            .OrderBy(b => b.Title)
            .Select(b => new BookSearchResponse(
                b.Id,
                b.Isbn,
                b.Title,
                b.Author,
                b.AvailableCopies,
                b.TotalCopies))
            .ToListAsync(cancellationToken);
    }

    private static bool MatchesLikePattern(string source, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || pattern == "%" || pattern == "%%")
        {
            return true;
        }

        if (!pattern.Contains('%'))
        {
            return source.Contains(pattern, StringComparison.OrdinalIgnoreCase);
        }

        var startsWithWildcard = pattern.StartsWith('%');
        var endsWithWildcard = pattern.EndsWith('%');

        var tokens = pattern
            .Split('%', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (tokens.Length == 0)
        {
            return true;
        }

        var currentIndex = 0;

        if (!startsWithWildcard)
        {
            if (!source.StartsWith(tokens[0], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            currentIndex = tokens[0].Length;
        }

        for (var i = startsWithWildcard ? 0 : 1; i < tokens.Length; i++)
        {
            var token = tokens[i];
            var foundIndex = source.IndexOf(token, currentIndex, StringComparison.OrdinalIgnoreCase);
            if (foundIndex < 0)
            {
                return false;
            }

            currentIndex = foundIndex + token.Length;
        }

        if (!endsWithWildcard)
        {
            return source.EndsWith(tokens[^1], StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    public async Task<LibraryActionResult> BorrowBookAsync(
        int bookId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var book = await db.Books.FindAsync([bookId], cancellationToken);
        if (book is null)
        {
            return LibraryActionResult.NotFound("Book not found.");
        }

        if (book.AvailableCopies <= 0)
        {
            return LibraryActionResult.Conflict("No copies are currently available.");
        }

        var existingLoan = await db.Loans
            .AnyAsync(l => l.BookId == bookId && l.UserId == userId && l.ReturnedAtUtc == null, cancellationToken);

        if (existingLoan)
        {
            return LibraryActionResult.Conflict("You already have this book borrowed.");
        }

        book.AvailableCopies -= 1;
        db.Loans.Add(new Loan
        {
            BookId = bookId,
            UserId = userId,
            BorrowedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);

        return LibraryActionResult.Success(new BookTransactionResponse(
            "Book borrowed successfully.",
            book.Id,
            book.Title,
            book.AvailableCopies));
    }

    public async Task<LibraryActionResult> ReturnBookAsync(
        int bookId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var loan = await db.Loans
            .FirstOrDefaultAsync(
                l => l.BookId == bookId && l.UserId == userId && l.ReturnedAtUtc == null,
                cancellationToken);

        if (loan is null)
        {
            return LibraryActionResult.NotFound("You do not currently have this book borrowed.");
        }

        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == loan.BookId, cancellationToken);
        if (book is null)
        {
            return LibraryActionResult.NotFound("Book not found.");
        }

        loan.ReturnedAtUtc = DateTime.UtcNow;
        book.AvailableCopies += 1;

        await db.SaveChangesAsync(cancellationToken);

        return LibraryActionResult.Success(new BookTransactionResponse(
            "Book returned successfully.",
            book.Id,
            book.Title,
            book.AvailableCopies));
    }

    public async Task<IReadOnlyList<BorrowedBookResponse>> GetBorrowedBooksAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var activeLoans = await db.Loans
            .AsNoTracking()
            .Where(l => l.UserId == userId && l.ReturnedAtUtc == null)
            .OrderByDescending(l => l.BorrowedAtUtc)
            .ToListAsync(cancellationToken);

        if (activeLoans.Count == 0)
        {
            return [];
        }

        var borrowedBookIds = activeLoans.Select(l => l.BookId).Distinct().ToList();
        var booksById = await db.Books
            .AsNoTracking()
            .Where(b => borrowedBookIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, cancellationToken);

        return activeLoans
            .Where(loan => booksById.ContainsKey(loan.BookId))
            .Select(loan =>
            {
                var book = booksById[loan.BookId];
                return new BorrowedBookResponse(
                    loan.BookId,
                    book.Isbn,
                    book.Title,
                    book.Author,
                    loan.BorrowedAtUtc);
            })
            .ToList();
    }
}