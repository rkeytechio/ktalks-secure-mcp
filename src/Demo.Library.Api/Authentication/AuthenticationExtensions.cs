using Demo.Library.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Demo.Library.Api.Authentication;

internal static class AuthenticationExtensions
{
    public static IServiceCollection AddLibraryAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authSection = configuration.GetSection(EntraAuthenticationOptions.SectionName);
        var authOptions = authSection.Get<EntraAuthenticationOptions>() ?? new EntraAuthenticationOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{authOptions.Instance.TrimEnd('/')}/{authOptions.TenantId}/v2.0";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = authOptions.Audience
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(LibraryAuthorizationPolicies.ApiAccountScopePolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context => context.User.HasScope(authOptions.RequiredApiScope));
            });
        });

        return services;
    }

    public static IApplicationBuilder UseLibraryAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}