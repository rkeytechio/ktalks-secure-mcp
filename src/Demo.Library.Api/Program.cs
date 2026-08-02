using Demo.Library.Api.Endpoints;
using Demo.Library.Api.Logging;
using Demo.Library.Api.Persistence;
using Demo.Library.Api.Persistence.Seed;
using Demo.Library.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// API and diagnostics
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Domain services
builder.Services.AddScoped<ILibraryService, LibraryService>();

// Activity logging
builder.Services.AddActivityLogging(builder.Configuration);

// Persistence
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

LibrarySeedData.Seed(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseActivityLogging();

app.MapLibraryEndpoints();

app.Run();
