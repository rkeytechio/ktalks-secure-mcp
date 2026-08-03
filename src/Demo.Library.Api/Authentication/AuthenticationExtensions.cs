using Demo.Library.Api.Endpoints;
using Demo.Library.Api.Mcp;
using Demo.Library.Api.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;
using System.Globalization;

namespace Demo.Library.Api.Authentication;

/// <summary>
/// Authentication and authorization wiring for API and MCP surfaces.
/// </summary>
/// <remarks>
/// # Secure MCP Design Note:
/// This class centralizes security-critical configuration for:
/// - JWT token validation for regular API endpoints,
/// - MCP protected-resource metadata publication,
/// - API and MCP scope-based authorization policies.
///
/// Security behavior implemented here:
/// 1) JWT bearer validates access tokens for API requests.
/// 2) JWT challenge responses include a resource_metadata link based on
///    EntraAuthentication:JwtResourceMetadataPath.
/// 3) AddMcp(...) publishes MCP protected-resource metadata from
///    Mcp:ResourceMetadataPath, including RequiredMcpScope.
/// 4) API and MCP currently share the same Audience as protected resource ID.
/// 5) LibraryApiAccountScopePolicyName enforces RequiredApiScope.
/// 6) LibraryMcpScopePolicyName enforces RequiredMcpScope.
/// 7) Route-level MCP authorization mode is configured in McpExtensions and can
///    override tool-level anonymity.
///
/// Keeping these concerns together makes security review easier and helps ensure
/// token validation, metadata advertisement, and policy enforcement stay aligned.
/// </remarks>
internal static class AuthenticationExtensions
{
    /// <summary>
    /// Configures authentication and authorization according to the class-level secure design note.
    /// </summary>
    public static IServiceCollection AddLibraryAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authSection = configuration.GetSection(EntraAuthenticationOptions.SectionName);
        var authOptions = authSection.Get<EntraAuthenticationOptions>() ?? new EntraAuthenticationOptions();
        var mcpOptions = configuration
            .GetSection(McpServerOptions.SectionName)
            .Get<McpServerOptions>()
            ?? new McpServerOptions();

        var authority = authOptions.Authority;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = authOptions.Audience
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        // Advertise API protected-resource metadata so OAuth-aware clients can
                        // discover auth server and scopes after a 401 challenge.
                        var metadataPath = authOptions.JwtResourceMetadataPath
                            .EnsureStartsWithSlash("/.well-known/oauth-protected-resource/api");
                        var metadataUri = new UriBuilder(
                            context.Request.Scheme,
                            context.Request.Host.Host,
                            context.Request.Host.Port ?? -1,
                            metadataPath).Uri;

                        var challengeValue = string.Format(
                            CultureInfo.InvariantCulture,
                            "Bearer resource_metadata=\"{0}\"",
                            metadataUri);

                        context.Response.Headers.Append("WWW-Authenticate", challengeValue);
                        return Task.CompletedTask;
                    }
                };
            })
            // # Secure MCP Design Note:
            // AddMcp(...) configures OAuth protected-resource metadata specifically for MCP clients.
            // This keeps MCP auth discovery explicit and separate from regular API endpoint docs.
            //
            // Why this matters in a secure MCP implementation:
            // - MCP clients receive a dedicated metadata endpoint (Mcp:ResourceMetadataPath),
            // - the metadata advertises MCP scope requirements (RequiredMcpScope),
            // - API and MCP can share audience while exposing protocol-specific documentation links.
            //
            // Security boundary clarification:
            // - AddMcp(ResourceMetadata*) advertises how to authenticate,
            // - authorization is still enforced by endpoint/tool policies and scope checks.
            //
            // Keep this configuration close to AddJwtBearer so token validation and metadata
            // advertisement stay aligned and are reviewed together.
            .AddMcp(options =>
            {
                // Pin MCP metadata to its own well-known endpoint so MCP and API metadata
                // can be discovered independently.
                options.ResourceMetadataUri = new Uri(
                    mcpOptions.ResourceMetadataPath
                        .EnsureStartsWithSlash("/.well-known/oauth-protected-resource/mcp"),
                    UriKind.Relative);

                options.ResourceMetadata = new()
                {
                    Resource = authOptions.Audience,
                    ResourceDocumentation = mcpOptions.ResourceDocumentationUrl,
                    AuthorizationServers = { authority },
                    ScopesSupported = [authOptions.RequiredMcpScope]
                };
            });

        // # Secure MCP Design Note:
        // Keep API and MCP authorization policies separate, even when they share issuer/audience.
        // API and MCP often expose different capabilities and risk levels:
        // - API endpoints may allow broader operational actions,
        // - MCP tools may include user-scoped and model-invoked workflows with different blast radius.
        //
        // Using distinct scopes/policies prevents cross-surface token reuse.
        // A token issued only for MCP scope should not authorize API operations, and vice versa.
        // This preserves least privilege and allows independent permission evolution per surface.
        services.AddAuthorization(options =>
        {
            options.AddPolicy(LibraryAuthorizationPolicies.ApiAccountScopePolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context => context.User.HasScope(authOptions.RequiredApiScope));
            });

            options.AddPolicy(LibraryAuthorizationPolicies.McpScopePolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context => context.User.HasScope(authOptions.RequiredMcpScope));
            });
        });

        return services;
    }

    /// <summary>
    /// Maps the OAuth protected-resource metadata endpoint for non-MCP API clients.
    /// </summary>
    /// <remarks>
    /// This endpoint is intentionally excluded from Swagger because it is protocol metadata,
    /// not a business API surface. It remains publicly reachable for standards-based discovery.
    /// </remarks>
    public static IEndpointRouteBuilder MapJwtProtectedResourceMetadata(this IEndpointRouteBuilder app, IConfiguration configuration)
    {
        var authOptions = configuration.GetSection(EntraAuthenticationOptions.SectionName).Get<EntraAuthenticationOptions>()
            ?? new EntraAuthenticationOptions();

        var authority = authOptions.Authority;
        var metadataPath = authOptions.JwtResourceMetadataPath
            .EnsureStartsWithSlash("/.well-known/oauth-protected-resource/api");

        app.MapGet(metadataPath, () => Results.Json(new Dictionary<string, object?>
        {
            ["resource"] = authOptions.Audience,
            ["resource_documentation"] = authOptions.ApiResourceDocumentationUrl,
            ["authorization_servers"] = new[] { authority },
            ["scopes_supported"] = new[] { authOptions.RequiredApiScope }
        }))
        .ExcludeFromDescription();

        return app;
    }

    public static IApplicationBuilder UseLibraryAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}