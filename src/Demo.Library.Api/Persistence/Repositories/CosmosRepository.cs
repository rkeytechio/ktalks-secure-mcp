using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Demo.Library.Api.Persistence.Abstractions;
using Demo.Library.Api.Persistence.Options;

namespace Demo.Library.Api.Persistence.Repositories;

internal sealed class CosmosRepository<TEntity>(
    CosmosClient cosmosClient,
    IOptions<CosmosDatabaseOptions> options,
    ICosmosContainerConfiguration<TEntity> containerConfiguration,
    ILogger<CosmosRepository<TEntity>> logger) : ICosmosRepository<TEntity>
    where TEntity : class, ICosmosEntity
{
    private readonly CosmosClient cosmosClient = cosmosClient;
    private readonly CosmosDatabaseOptions options = options.Value;
    private readonly ICosmosContainerConfiguration<TEntity> containerConfiguration = containerConfiguration;
    private readonly ILogger<CosmosRepository<TEntity>> logger = logger;
    private readonly SemaphoreSlim initializeLock = new(1, 1);

    private Container? container;

    public async Task CreateItemAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetContainer = await GetContainerAsync(cancellationToken);
            await targetContainer.CreateItemAsync(
                entity,
                new PartitionKey(entity.PartitionKey),
                cancellationToken: cancellationToken);
        }
        catch (CosmosException ex)
        {
            logger.LogError(ex, "Cosmos failed while writing entity {EntityType}.", typeof(TEntity).Name);
        }
    }

    private async Task<Container> GetContainerAsync(CancellationToken cancellationToken)
    {
        if (container is not null)
        {
            return container;
        }

        await initializeLock.WaitAsync(cancellationToken);
        try
        {
            if (container is not null)
            {
                return container;
            }

            if (options.EnsureCreated)
            {
                var databaseResponse = await cosmosClient
                    .CreateDatabaseIfNotExistsAsync(options.DatabaseName, cancellationToken: cancellationToken);

                var containerResponse = await databaseResponse.Database
                    .CreateContainerIfNotExistsAsync(
                        new ContainerProperties(containerConfiguration.ContainerName, containerConfiguration.PartitionKeyPath),
                        cancellationToken: cancellationToken);

                container = containerResponse.Container;
            }
            else
            {
                container = cosmosClient.GetContainer(options.DatabaseName, containerConfiguration.ContainerName);
            }

            return container;
        }
        finally
        {
            initializeLock.Release();
        }
    }
}