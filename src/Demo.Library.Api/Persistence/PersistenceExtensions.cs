using Demo.Library.Api.Persistence.Options;
using Azure.Identity;
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

        services.AddDbContext<LibraryDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(cosmosOptions.ConnectionString))
            {
                options.UseCosmos(cosmosOptions.ConnectionString, cosmosOptions.DatabaseName);
                return;
            }

            if (string.IsNullOrWhiteSpace(cosmosOptions.AccountEndpoint))
            {
                throw new InvalidOperationException(
                    "Either CosmosDatabase:ConnectionString or CosmosDatabase:AccountEndpoint must be configured.");
            }

            var credential = string.IsNullOrWhiteSpace(cosmosOptions.ManagedIdentityClientId)
                ? new DefaultAzureCredential()
                : new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = cosmosOptions.ManagedIdentityClientId
                });

            options.UseCosmos(cosmosOptions.AccountEndpoint, credential, cosmosOptions.DatabaseName);
        });

        return services;
    }
}
