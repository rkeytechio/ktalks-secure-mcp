using Demo.Library.Api.Data;
using Demo.Library.Api.Endpoints;
using Demo.Library.Api.Logging;
using Demo.Library.Api.Persistence;
using Demo.Library.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// API and diagnostics
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Domain services
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseInMemoryDatabase("LibraryDemoDb"));
builder.Services.AddScoped<ILibraryService, LibraryService>();

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

app.MapLibraryEndpoints();

app.Run();
