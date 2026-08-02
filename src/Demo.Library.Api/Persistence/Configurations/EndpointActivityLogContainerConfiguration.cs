using Microsoft.Extensions.Options;
using Demo.Library.Api.Persistence.Abstractions;
using Demo.Library.Api.Persistence.Entities;
using Demo.Library.Api.Persistence.Options;

namespace Demo.Library.Api.Persistence.Configurations;

internal sealed class EndpointActivityLogContainerConfiguration(IOptions<CosmosDatabaseOptions> options)
    : ICosmosContainerConfiguration<EndpointActivityLog>
{
    private readonly CosmosDatabaseOptions options = options.Value;

    public string ContainerName => this.options.EndpointActivityContainerName;
}
