using BooksApi.Models;
using BooksApi.Services;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://localhost:5000");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<BookStore>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: "frontend",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.Use(async (ctx, next) =>
    {
        Console.WriteLine($"{DateTimeOffset.Now:HH:mm:ss} {ctx.Request.Method} {ctx.Request.Path}");
        await next();
    });
}

app.UseCors("frontend");

var booksApi = app.MapGroup("/api/books").WithTags("Books");

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

booksApi.MapGet("", (BookStore store) => Results.Ok(store.GetAll()));

booksApi.MapGet(
    "/{id:int}",
    (int id, BookStore store) =>
    {
        var book = store.GetById(id);
        return book is null ? Results.NotFound() : Results.Ok(book);
    });

booksApi.MapPost(
    "",
    (BookUpsertDto dto, BookStore store, HttpContext http) =>
    {
        var validation = BookValidation.Validate(dto);
        if (validation is not null) return validation;

        var created = store.Create(dto);
        return Results.Created($"/api/books/{created.Id}", created);
    });

booksApi.MapPut(
    "/{id:int}",
    (int id, BookUpsertDto dto, BookStore store) =>
    {
        var validation = BookValidation.Validate(dto);
        if (validation is not null) return validation;

        var updated = store.Update(id, dto);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    });

booksApi.MapDelete(
    "/{id:int}",
    (int id, BookStore store) => store.Delete(id) ? Results.NoContent() : Results.NotFound());

app.Run();
