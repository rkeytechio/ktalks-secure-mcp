using Demo.Library.Api.Endpoints.Me.Contracts;
using Demo.Library.Api.Persistence.Entities;

namespace Demo.Library.Api.Models;

internal static class EntityModelMappings
{
    public static Book ToModel(this BookEntity entity)
    {
        return new Book
        {
            Id = entity.Id,
            Isbn = entity.Isbn,
            Title = entity.Title,
            Author = entity.Author,
            TotalCopies = entity.TotalCopies,
            AvailableCopies = entity.AvailableCopies
        };
    }

    public static Loan ToModel(this LoanEntity entity, BookEntity? bookEntity = null)
    {
        return new Loan
        {
            Id = entity.Id,
            BookId = entity.BookId,
            Book = (bookEntity ?? entity.Book)?.ToModel(),
            UserId = entity.UserId,
            BorrowedAtUtc = entity.BorrowedAtUtc,
            ReturnedAtUtc = entity.ReturnedAtUtc
        };
    }

    public static BorrowedBookResponse ToBorrowedBookResponse(this Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan.Book);

        return new BorrowedBookResponse(
            loan.BookId,
            loan.Book.Isbn,
            loan.Book.Title,
            loan.Book.Author,
            loan.BorrowedAtUtc);
    }
}
