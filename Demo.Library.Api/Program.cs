using Demo.Library.Api.Data;
using Demo.Library.Api.Endpoints;
using Demo.Library.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseInMemoryDatabase("LibraryDemoDb"));
builder.Services.AddScoped<ILibraryService, LibraryService>();

var app = builder.Build();

LibrarySeedData.Seed(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapLibraryEndpoints();

app.Run();
