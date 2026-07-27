using Demo.Library.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Library.Api.Data;

internal sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Loan> Loans => Set<Loan>();
}
