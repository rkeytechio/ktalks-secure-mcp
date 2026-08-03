using Microsoft.Extensions.Configuration;

namespace Demo.Library.Api.Persistence.Options;

internal sealed class CosmosDatabaseOptions
{
    public const string SectionName = "CosmosDatabase";
    public const string EndpointActivity = "EndpointActivity";
    public const string Books = "Books";
    public const string Loans = "Loans";
    public const string AccountClosureRequests = "AccountClosureRequests";

    public string? ConnectionString { get; set; }

    public string? AccountEndpoint { get; set; }

    public string? ManagedIdentityClientId { get; set; }

    public string DatabaseName { get; set; } = "DemoLibrary";

    [ConfigurationKeyName("EndpointActivity")]
    public string EndpointActivityContainerName { get; set; } = EndpointActivity;

    [ConfigurationKeyName("Books")]
    public string BooksContainerName { get; set; } = Books;

    [ConfigurationKeyName("Loans")]
    public string LoansContainerName { get; set; } = Loans;

    [ConfigurationKeyName("AccountClosureRequests")]
    public string AccountClosureRequestsContainerName { get; set; } = AccountClosureRequests;

    public bool EnsureCreated { get; set; } = true;
}