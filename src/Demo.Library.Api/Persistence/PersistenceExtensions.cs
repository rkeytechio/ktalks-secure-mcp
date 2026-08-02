using Demo.Library.Api.Persistence.Options;
using Microsoft.EntityFrameworkCore;

namespace Demo.Library.Api.Persistence;

internal static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CosmosDatabaseOptions>(
            configuration.GetSection(CosmosDatabaseOptions.SectionName));

        var cosmosOptions = configuration
            .GetSection(CosmosDatabaseOptions.SectionName)
            .Get<CosmosDatabaseOptions>() ?? new CosmosDatabaseOptions();

        if (string.IsNullOrWhiteSpace(cosmosOptions.ConnectionString))
        {
            throw new InvalidOperationException("CosmosDatabase:ConnectionString is required.");
        }

        services.AddDbContext<LibraryDbContext>(options =>
            options.UseCosmos(cosmosOptions.ConnectionString, cosmosOptions.DatabaseName));

        return services;
    }
}
