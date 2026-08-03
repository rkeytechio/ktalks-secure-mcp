namespace Demo.Library.Api.Mcp;

// # Secure MCP Design Note:
// These options define MCP server identity, transport behavior, metadata discovery endpoint,
// and route-level authorization mode. Treat them as security-sensitive deployment settings.

internal enum McpAuthorizationMode
{
    ToolLevel = 0,
    RequireAuthForAllRequests = 1
}

internal sealed class McpServerOptions
{
    public const string SectionName = "Mcp";

    public string ServerName { get; set; } = "demo-library-mcp";

    public string ServerVersion { get; set; } = "1.0.0";

    public bool StatelessTransport { get; set; } = true;

    public string ResourceDocumentationUrl { get; set; } = "https://docs.example.com/api/library-mcp";

    public string ResourceMetadataPath { get; set; } = "/.well-known/oauth-protected-resource/mcp";

    public McpAuthorizationMode AuthorizationMode { get; set; } = McpAuthorizationMode.ToolLevel;
}