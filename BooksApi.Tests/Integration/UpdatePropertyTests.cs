using System;
using System.Threading.Tasks;
using BooksApi.Data;
using BooksApi.DTOs;
using BooksApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace BooksApi.Tests.Integration;

/// <summary>
/// Property-based tests for update operation correctness.
/// Tests Property 3: Update reflects submitted fields
/// </summary>
public class UpdatePropertyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UpdatePropertyTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // Feature: books-api, Property 3: Update reflects submitted fields
    [Property]
    public async Property Update_ReflectsSubmittedFields()
    {
        // Generate a random valid UpdateBookDto
        var gen = from title in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 200)
                  from author in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from pages in Arb.Generate<int>().Where(p => p >= 1 && p <= 99999)
                  from language in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from genre in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from coverUrl in Arb.Generate<string?>().Where(u => u == null || !string.IsNullOrWhiteSpace(u))
                  select new UpdateBookDto
                  {
                      Title = title,
                      Author = author,
                      Pages = pages,
                      Language = language,
                      Genre = genre,
                      CoverImageUrl = coverUrl
                  };

        // First seed a book using the test database context
        await Property.ForAll(gen)
            .Do(async dto =>
            {
                // Create a test database context to seed a book
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                var options = new DbContextOptionsBuilder<BookDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using var dbContext = new BookDbContext(options);
                dbContext.Database.EnsureCreated();

                // Seed a book with known values
                var seedBook = new Book
                {
                    Title = "Original Title",
                    Author = "Original Author",
                    Pages = 100,
                    Language = "English",
                    Genre = "Fiction",
                    CoverImageUrl = "http://original.jpg"
                };

                dbContext.Books.Add(seedBook);
                await dbContext.SaveChangesAsync();

                // Get the Id of the seeded book
                var seededId = seedBook.Id;

                // Create a client with test database context
                var client = CreateClientWithSeed(connection);

                // Perform PUT request
                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var putResponse = await client.PutAsync($"/api/books/{seededId}", content);

                Assert.Equal(System.Net.HttpStatusCode.Ok, putResponse.StatusCode);

                // Verify response body contains updated values
                var putResponseBody = await putResponse.Content.ReadAsStringAsync();
                var updatedBookFromResponse = System.Text.Json.JsonSerializer.Deserialize<Book>(putResponseBody);
                Assert.NotNull(updatedBookFromResponse);
                Assert.Equal(dto.Title, updatedBookFromResponse!.Title);
                Assert.Equal(dto.Author, updatedBookFromResponse.Author);
                Assert.Equal(dto.Pages, updatedBookFromResponse.Pages);
                Assert.Equal(dto.Language, updatedBookFromResponse.Language);
                Assert.Equal(dto.Genre, updatedBookFromResponse.Genre);
                Assert.Equal(dto.CoverImageUrl, updatedBookFromResponse.CoverImageUrl);

                // Now GET the book to verify persistence
                var getResponse = await client.GetAsync($"/api/books/{seededId}");
                Assert.Equal(System.Net.HttpStatusCode.Ok, getResponse.StatusCode);

                var getResponseBody = await getResponse.Content.ReadAsStringAsync();
                var bookFromGet = System.Text.Json.JsonSerializer.Deserialize<Book>(getResponseBody);
                Assert.NotNull(bookFromGet);
                Assert.Equal(dto.Title, bookFromGet!.Title);
                Assert.Equal(dto.Author, bookFromGet.Author);
                Assert.Equal(dto.Pages, bookFromGet.Pages);
                Assert.Equal(dto.Language, bookFromGet.Language);
                Assert.Equal(dto.Genre, bookFromGet.Genre);
                Assert.Equal(dto.CoverImageUrl, bookFromGet.CoverImageUrl);

                connection.Close();
            })
            .QuickCheckThrowOnFailure();
    }

    private HttpClient CreateClientWithSeed(SqliteConnection connection)
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(DbContextOptions<BookDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<BookDbContext>(options =>
                {
                    options.UseSqlite(connection);
                });
            });

            builder.UseSetting("ConnectionStrings:DefaultConnection", connection.ConnectionString);
        }).CreateClient();

        return client;
    }
}
