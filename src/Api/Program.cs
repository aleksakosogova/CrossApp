using Api.Filters;
using Api.Middleware;
using Api.Endpoints;
using Core.Abstractions;
using Core.Storage;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Реєстрація OpenAPI, ProblemDetails та сховища
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ICatalogStore, InMemoryCatalogStore>();

// Реєстрація FluentValidation та обробника винятків (Додаткові завдання 2 та 3)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Використання глобального обробника винятків та статусів
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); //[cite: 1, 4]
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapProducts();

app.Run();