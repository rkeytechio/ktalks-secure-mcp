using Demo.Library.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Demo.Library.Api.Persistence.Seed;

internal static class LibrarySeedData
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        await db.Database.EnsureCreatedAsync();

        var existingBookIds = await db.Books
            .Select(book => book.Id)
            .Take(1)
            .ToListAsync();

        if (existingBookIds.Count > 0)
        {
            return;
        }

        var books = new[]
        {
            new BookEntity { Isbn = "978-1617295416", Title = "C# in Depth", Author = "Jon Skeet", TotalCopies = 4, AvailableCopies = 4 },
            new BookEntity { Isbn = "978-1617295829", Title = "ASP.NET Core in Action", Author = "Andrew Lock", TotalCopies = 3, AvailableCopies = 3 },
            new BookEntity { Isbn = "978-1617296574", Title = "gRPC in .NET", Author = "Maarten Balliauw", TotalCopies = 2, AvailableCopies = 2 },
            new BookEntity { Isbn = "978-1484289204", Title = "Pro Entity Framework Core 8", Author = "Adam Freeman", TotalCopies = 2, AvailableCopies = 2 },
            new BookEntity { Isbn = "978-1492097549", Title = "Learning Blazor", Author = "David Pine", TotalCopies = 3, AvailableCopies = 3 },
            new BookEntity { Isbn = "978-1484278680", Title = ".NET MAUI in Action", Author = "Matt Lacey", TotalCopies = 2, AvailableCopies = 2 },
            new BookEntity { Isbn = "978-1803236180", Title = "Minimal APIs in ASP.NET Core", Author = "Nick Proud", TotalCopies = 4, AvailableCopies = 4 },
            new BookEntity { Isbn = "978-1617296604", Title = "Unit Testing in C#", Author = "Benjamin Johnson", TotalCopies = 3, AvailableCopies = 3 },
            new BookEntity { Isbn = "978-1617294532", Title = "Concurrency in C# Cookbook", Author = "Stephen Cleary", TotalCopies = 2, AvailableCopies = 2 },
            new BookEntity { Isbn = "978-1098113322", Title = "Designing Data-Intensive Applications", Author = "Martin Kleppmann", TotalCopies = 2, AvailableCopies = 2 },
            new BookEntity { Isbn = "978-0131103627", Title = "The C Programming Language", Author = "Brian W. Kernighan", TotalCopies = 1, AvailableCopies = 1 },
            new BookEntity { Isbn = "978-0132350884", Title = "Clean Code", Author = "Robert C. Martin", TotalCopies = 4, AvailableCopies = 4 },
            new BookEntity { Isbn = "978-0134494166", Title = "Clean Architecture", Author = "Robert C. Martin", TotalCopies = 3, AvailableCopies = 3 },
            new BookEntity { Isbn = "978-0201633610", Title = "Design Patterns", Author = "Erich Gamma", TotalCopies = 2, AvailableCopies = 2 },
            new BookEntity { Isbn = "978-1492056812", Title = "Software Architecture: The Hard Parts", Author = "Neal Ford", TotalCopies = 2, AvailableCopies = 2 },
            new BookEntity { Isbn = "978-1617296277", Title = "Microservices Patterns", Author = "Chris Richardson", TotalCopies = 3, AvailableCopies = 3 },
            new BookEntity { Isbn = "978-1492078005", Title = "Kubernetes: Up and Running", Author = "Brendan Burns", TotalCopies = 2, AvailableCopies = 2 },
            new BookEntity { Isbn = "978-1491950357", Title = "Site Reliability Engineering", Author = "Betsy Beyer", TotalCopies = 2, AvailableCopies = 2 },
            new BookEntity { Isbn = "978-1801079413", Title = "Mastering Azure Architecture", Author = "Ritesh Modi", TotalCopies = 3, AvailableCopies = 3 },
            new BookEntity { Isbn = "978-1492032649", Title = "Programming C# 8.0", Author = "Ian Griffiths", TotalCopies = 3, AvailableCopies = 3 }
        };

        db.Books.AddRange(books);

        await db.SaveChangesAsync();
    }
}
