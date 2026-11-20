using System.Text.Json;
using Api.Endpoints;
using Api.Middleware;
using Core.Abstractions;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Явний camelCase для JSON на рівні HTTP API
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Єдиний формат помилок ProblemDetails + глобальний обробник винятків
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Конфігурація рядка підключення з абсолютним шляхом
var raw = builder.Configuration.GetConnectionString("AppDb") ?? "Data Source=data/app.db";
var csb = new SqliteConnectionStringBuilder(raw);

var isTesting = builder.Environment.IsEnvironment("Testing") ||
                AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name?.Contains("Tests") == true);

if (isTesting)
{
    // Генеруємо унікальну базу для кожного тестового запуску всередині папки data
    var testDbName = $"test_app_{Guid.NewGuid()}.db";
    var fullPath = Path.Combine(builder.Environment.ContentRootPath, "data", testDbName);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    csb.DataSource = fullPath;
}
else if (!Path.IsPathRooted(csb.DataSource))
{
    var full = Path.Combine(builder.Environment.ContentRootPath, csb.DataSource);
    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
    csb.DataSource = full;
}
else
{
    Directory.CreateDirectory(Path.GetDirectoryName(csb.DataSource)!);
}

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(csb.ConnectionString));
builder.Services.AddScoped<IAsyncCatalogStore, EfCatalogStore>();

var webOrigin = builder.Configuration["Cors:WebOrigin"] ?? "http://localhost:5090";
builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy =>
        policy.WithOrigins(webOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Автоматичне застосування міграцій при старті
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsDevelopment() || isTesting)
    {
        db.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("web"); // Обов'язково до app.MapProducts()[cite: 4]

app.MapProducts();
app.MapControllers();

app.Run();

public partial class Program { }
