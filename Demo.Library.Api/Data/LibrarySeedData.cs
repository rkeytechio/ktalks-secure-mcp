using Demo.Library.Api.Models;

namespace Demo.Library.Api.Data;

internal static class LibrarySeedData
{
    public static void Seed(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        db.Database.EnsureCreated();

        if (db.Books.Any())
        {
            return;
        }

        db.Books.AddRange(
            new Book { Isbn = "978-1617295416", Title = "C# in Depth", Author = "Jon Skeet", TotalCopies = 4, AvailableCopies = 4 },
            new Book { Isbn = "978-1617295829", Title = "ASP.NET Core in Action", Author = "Andrew Lock", TotalCopies = 3, AvailableCopies = 3 },
            new Book { Isbn = "978-1617296574", Title = "gRPC in .NET", Author = "Maarten Balliauw", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Isbn = "978-1484289204", Title = "Pro Entity Framework Core 8", Author = "Adam Freeman", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Isbn = "978-1492097549", Title = "Learning Blazor", Author = "David Pine", TotalCopies = 3, AvailableCopies = 3 },
            new Book { Isbn = "978-1484278680", Title = ".NET MAUI in Action", Author = "Matt Lacey", TotalCopies = 2, AvailableCopies = 2 },
            new Book { Isbn = "978-1803236180", Title = "Minimal APIs in ASP.NET Core", Author = "Nick Proud", TotalCopies = 4, AvailableCopies = 4 },
            new Book { Isbn = "978-1617296604", Title = "Unit Testing in C#", Author = "Benjamin Johnson", TotalCopies = 3, AvailableCopies = 3 });

        db.SaveChanges();
    }
}
