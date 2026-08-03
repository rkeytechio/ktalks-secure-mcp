namespace Demo.Library.Api.Mcp;

using Demo.Library.Api.Authentication;

internal static class McpExtensions
{
    public static IServiceCollection AddLibraryMcp(this IServiceCollection services, IConfiguration configuration)
    {
        var mcpOptions = configuration
            .GetSection(McpServerOptions.SectionName)
            .Get<McpServerOptions>()
            ?? new McpServerOptions();

        // Required for MCP tools that read current user identity from HttpContext.
        services.AddHttpContextAccessor();

        services.AddMcpServer(options =>
            {
                options.ServerInfo = new()
                {
                    Name = mcpOptions.ServerName,
                    Version = mcpOptions.ServerVersion
                };
            })
            .WithHttpTransport(transport =>
            {
                // Stateless mode is enough for request/response tools and is easier to scale.
                transport.Stateless = mcpOptions.StatelessTransport;
            })
            // # Secure MCP Design Note:
            // Tool registration can be done in a minimal way and both patterns are valid:
            // - typed registration:
            //      services
            //          .AddMcpServer()
            //          .WithHttpTransport()
            //          .WithTools<LibraryTools>();

            // - assembly registration:
            //      services
            //          .AddMcpServer()
            //          .WithHttpTransport()
            //          .WithToolsFromAssembly();
            //
            // For a secure MCP design, add AddAuthorizationFilters() before registering tools.
            // This ensures ASP.NET [Authorize]/[AllowAnonymous] metadata is applied in MCP flows.
            //
            // Security outcomes:
            // - tools/list is filtered per caller identity and permissions,
            // - tool calls are policy-checked before execution,
            // - public and protected tools can safely coexist in the same tool type.
            //
            // Keep AddAuthorizationFilters() adjacent to tool registration so discovery and
            // invocation for that tool set are consistently authorization-aware.
            .AddAuthorizationFilters()
            .WithTools<LibraryTools>();

        return services;
    }

    public static IEndpointRouteBuilder MapLibraryMcp(this IEndpointRouteBuilder app, IConfiguration configuration)
    {
        var mcpOptions = configuration
            .GetSection(McpServerOptions.SectionName)
            .Get<McpServerOptions>()
            ?? new McpServerOptions();

        // # Secure MCP Design Note:
        // AuthorizationMode defines where and how MCP access is enforced:
        // - RequireAuthForAllRequests:
        //   Route-level authorization is applied to /mcp, including tools/list discovery.
        //   This is the strict mode for fully protected MCP deployments.
        // - ToolLevel:
        //   The /mcp route remains reachable and [Authorize]/[AllowAnonymous] is enforced per tool
        //   through MCP authorization filters. This is the mixed-access mode for public + protected
        //   tool sets in the same server.
        //
        // Choose the mode intentionally per deployment risk model.
        if (mcpOptions.AuthorizationMode == McpAuthorizationMode.RequireAuthForAllRequests)
        {
            app.MapMcp("/mcp")
                .RequireAuthorization(LibraryAuthorizationPolicies.McpScopePolicyName);
        }
        else
        {
            app.MapMcp("/mcp");
        }

        return app;
    }
}
