namespace Demo.Library.Api.Persistence.Abstractions;

internal interface ICosmosContainerConfiguration<TEntity>
    where TEntity : class, ICosmosEntity
{
    string ContainerName { get; }

    string PartitionKeyPath => "/PartitionKey";
}
