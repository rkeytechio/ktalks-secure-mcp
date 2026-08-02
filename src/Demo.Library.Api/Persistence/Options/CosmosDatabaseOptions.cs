using Microsoft.Extensions.Configuration;

namespace Demo.Library.Api.Persistence.Options;

internal sealed class CosmosDatabaseOptions
{
    public const string SectionName = "CosmosDatabase";
    public const string EndpointActivity = "EndpointActivity";
    public const string Books = "Books";
    public const string Loans = "Loans";

    public string? ConnectionString { get; set; }

    public string DatabaseName { get; set; } = "DemoLibrary";

    [ConfigurationKeyName("EndpointActivity")]
    public string EndpointActivityContainerName { get; set; } = EndpointActivity;

    [ConfigurationKeyName("Books")]
    public string BooksContainerName { get; set; } = Books;

    [ConfigurationKeyName("Loans")]
    public string LoansContainerName { get; set; } = Loans;

    public bool EnsureCreated { get; set; } = true;
}