namespace Demo.Library.Api.Authentication;

internal sealed class EntraAuthenticationOptions
{
    public const string SectionName = "EntraAuthentication";

    public string Instance { get; set; } = "https://login.microsoftonline.com";
    public string TenantId { get; set; } = "common";
    public string Audience { get; set; } = string.Empty;
    public string RequiredApiScope { get; set; } = "api.library.account";
    public string RequiredMcpScope { get; set; } = "mcp.library.account";
    public string ApiResourceDocumentationUrl { get; set; } = "https://docs.example.com/api/library-rest";
    public string JwtResourceMetadataPath { get; set; } = "/.well-known/oauth-protected-resource/api";

    public string Authority => $"{Instance.TrimEnd('/')}/{TenantId}/v2.0";
}
