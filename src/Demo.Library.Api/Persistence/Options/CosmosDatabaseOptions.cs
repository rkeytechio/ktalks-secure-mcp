namespace Demo.Library.Api.Persistence.Options;

internal sealed class CosmosDatabaseOptions
{
    public const string SectionName = "CosmosDatabase";

    public string? ConnectionString { get; set; }

    public string DatabaseName { get; set; } = "DemoLibrary";

    public string EndpointActivityContainerName { get; set; } = "EndpointActivity";

    public bool EnsureCreated { get; set; } = true;
}