using Demo.Library.Api.Models;
using Demo.Library.Api.Persistence.Entities;
using Demo.Library.Api.Persistence.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Demo.Library.Api.Persistence;

internal sealed class LibraryDbContext(
    DbContextOptions<LibraryDbContext> options,
    IOptions<CosmosDatabaseOptions> cosmosOptions) : DbContext(options)
{
    private readonly CosmosDatabaseOptions cosmosOptions = cosmosOptions.Value;

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<EndpointActivityLog> EndpointActivityLogs => Set<EndpointActivityLog>();
    public DbSet<AccountClosureRequestEntity> AccountClosureRequests => Set<AccountClosureRequestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToContainer(cosmosOptions.BooksContainerName);
            entity.HasKey(x => x.Id);
            entity.HasPartitionKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToContainer(cosmosOptions.LoansContainerName);
            entity.HasKey(x => x.Id);
            entity.HasPartitionKey(x => x.UserId);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Ignore(x => x.Book);
        });

        modelBuilder.Entity<EndpointActivityLog>(entity =>
        {
            entity.ToContainer(cosmosOptions.EndpointActivityContainerName);
            entity.HasKey(x => x.Id);
            entity.HasPartitionKey(x => x.ActivityType);
            entity.Property(x => x.Id).ToJsonProperty("id");
        });

        modelBuilder.Entity<AccountClosureRequestEntity>(entity =>
        {
            entity.ToContainer(cosmosOptions.AccountClosureRequestsContainerName);
            entity.HasKey(x => x.Id);
            entity.HasPartitionKey(x => x.UserId);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Status).HasConversion<string>();
        });
    }
}
