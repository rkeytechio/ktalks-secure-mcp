namespace Demo.Library.Api.Persistence.Abstractions;

internal interface ICosmosRepository<TEntity>
    where TEntity : class, ICosmosEntity
{
    Task CreateItemAsync(TEntity entity, CancellationToken cancellationToken = default);
}
