using Demo.Library.Api.Persistence.Entities;

namespace Demo.Library.Api.Persistence.Abstractions;

internal interface IActivityLogRepository
{
    Task SaveActivityAsync(EndpointActivityLog activityLog, CancellationToken cancellationToken = default);
}