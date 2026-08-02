using Demo.Library.Api.Persistence.Abstractions;
using Demo.Library.Api.Persistence.Configurations;
using Demo.Library.Api.Persistence.Entities;
using Demo.Library.Api.Persistence.Options;
using Demo.Library.Api.Persistence.Repositories;
using Microsoft.Azure.Cosmos;

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

        services.AddSingleton(_ => new CosmosClient(cosmosOptions.ConnectionString));
        services.AddSingleton(typeof(ICosmosRepository<>), typeof(CosmosRepository<>));
        services.AddSingleton<ICosmosContainerConfiguration<EndpointActivityLog>, EndpointActivityLogContainerConfiguration>();
        services.AddSingleton<IActivityLogRepository, ActivityLogRepository>();

        return services;
    }
}
