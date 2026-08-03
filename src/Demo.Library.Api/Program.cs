using Demo.Library.Api.Endpoints;
using Demo.Library.Api.Authentication;
using Demo.Library.Api.Logging;
using Demo.Library.Api.OpenApi;
using Demo.Library.Api.Persistence;
using Demo.Library.Api.Persistence.Seed;
using Demo.Library.Api.Services;
using Demo.Library.Api.Telemetry;

var builder = WebApplication.CreateBuilder(args);

// API and diagnostics
builder.Services.AddLibraryOpenApi();

// OpenTelemetry
builder.Services.AddLibraryOpenTelemetry(builder.Configuration);

// Authentication and authorization
builder.Services.AddLibraryAuthentication(builder.Configuration);

// Domain services
builder.Services.AddScoped<ILibraryService, LibraryService>();

// Activity logging
builder.Services.AddActivityLogging(builder.Configuration);

// Persistence
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

await LibrarySeedData.SeedAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline: HTTPS -> Auth -> Activity logging -> Endpoints.
app.UseHttpsRedirection();
app.UseLibraryAuthentication();
app.UseActivityLogging();
app.MapLibraryEndpoints();

app.Run();
