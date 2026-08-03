namespace Demo.Library.Api.Authentication;

internal sealed class EntraAuthenticationOptions
{
    public const string SectionName = "EntraAuthentication";

    public string Instance { get; set; } = "https://login.microsoftonline.com";
    public string TenantId { get; set; } = "common";
    public string Audience { get; set; } = string.Empty;
    public string RequiredApiScope { get; set; } = "api.library.account";
}
