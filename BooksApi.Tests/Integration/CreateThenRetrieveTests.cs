using System;
using System.Threading.Tasks;
using BooksApi.DTOs;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BooksApi.Tests.Integration;

// Feature: books-api, Property 1: Create-then-retrieve round trip
public class CreateThenRetrieveTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CreateThenRetrieveTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // Feature: books-api, Property 1: Create-then-retrieve round trip
    [Property(MaxTest = 100)]
    public async Property CreateThenRetrieve_RoundTrip_PreservesAllData()
    {
        // Generate a random valid CreateBookDto
        var gen = from title in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 500)
                  from author in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 300)
                  from pages in Arb.Generate<int>().Where(p => p >= 1 && p <= 50000)
                  from language in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from genre in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from coverImageUrl in Arb.Generate<string?>()
                  select new CreateBookDto
                  {
                      Title = title,
                      Author = author,
                      Pages = pages,
                      Language = language,
                      Genre = genre,
                      CoverImageUrl = coverImageUrl
                  };

        await Property.ForAll(gen)
            .Step(async (dto, _) =>
            {
                var client = _factory.CreateClient();

                // POST the book
                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var postResponse = await client.PostAsync("/api/books", content);

                Assert.Equal(System.Net.HttpStatusCode.Created, postResponse.StatusCode);

                // Parse the response to get the created book's Id
                var responseContent = await postResponse.Content.ReadAsStringAsync();
                var createdBook = System.Text.Json.JsonSerializer.Deserialize<BooksApi.Models.Book>(responseContent) 
                    ?? throw new InvalidOperationException("Response body is empty");

                // GET the book by Id
                var getResponse = await client.GetAsync($"/api/books/{createdBook.Id}");
                Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.StatusCode);

                // Verify all fields match
                var retrievedBook = System.Text.Json.JsonSerializer.Deserialize<BooksApi.Models.Book>(await getResponse.Content.ReadAsStringAsync())
                    ?? throw new InvalidOperationException("Response body is empty");

                Assert.Equal(dto.Title, retrievedBook.Title);
                Assert.Equal(dto.Author, retrievedBook.Author);
                Assert.Equal(dto.Pages, retrievedBook.Pages);
                Assert.Equal(dto.Language, retrievedBook.Language);
                Assert.Equal(dto.Genre, retrievedBook.Genre);
                Assert.Equal(dto.CoverImageUrl, retrievedBook.CoverImageUrl);
            })
            .QuickCheckThrowOnFailure();
    }
}
