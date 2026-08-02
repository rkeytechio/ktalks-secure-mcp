using Demo.Library.Api.Persistence.Abstractions;
using Demo.Library.Api.Persistence.Entities;

namespace Demo.Library.Api.Persistence.Repositories;

internal sealed class ActivityLogRepository(ICosmosRepository<EndpointActivityLog> repository)
    : IActivityLogRepository
{
    private readonly ICosmosRepository<EndpointActivityLog> repository = repository;

    public Task SaveActivityAsync(EndpointActivityLog activityLog, CancellationToken cancellationToken = default)
    {
        return repository.CreateItemAsync(activityLog, cancellationToken);
    }
}
