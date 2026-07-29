using System;
using System.Threading.Tasks;
using BooksApi.DTOs;
using BooksApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace BooksApi.Tests.Integration;

public class DeletePropertyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DeletePropertyTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // Feature: books-api, Property 4: Delete removes the record
    [Property]
    public async Property DeleteThenGetReturns404()
    {
        // Generate a valid CreateBookDto
        var gen = from title in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 500)
                  from author in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 300)
                  from pages in Arb.Generate<int>().Where(p => p >= 1 && p <= 50000)
                  from language in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from genre in Arb.Generate<string>().Where(s => !string.IsNullOrWhiteSpace(s))
                  from coverUrl in Arb.Generate<string?>().Where(s => s == null || !string.IsNullOrWhiteSpace(s))
                  select new CreateBookDto
                  {
                      Title = title,
                      Author = author,
                      Pages = pages,
                      Language = language,
                      Genre = genre,
                      CoverImageUrl = coverUrl
                  };

        await Property.ForAll(gen)
            .Step((dto, _) =>
            {
                var client = _factory.CreateClient();
                
                // Step 1: Create a book via POST
                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var postResponse = client.PostAsync("/api/books", content).Result;

                Assert.Equal(System.Net.HttpStatusCode.Created, postResponse.StatusCode);

                // Get the created book's ID from the response
                var postContent = postResponse.Content.ReadAsStringAsync().Result;
                var createdBook = System.Text.Json.JsonSerializer.Deserialize<Book>(postContent);
                Assert.NotNull(createdBook);
                var bookId = createdBook!.Id;

                // Step 2: Delete the book via DELETE
                var deleteResponse = client.DeleteAsync($"/api/books/{bookId}").Result;
                Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

                // Step 3: Verify the book no longer exists via GET
                var getResponse = client.GetAsync($"/api/books/{bookId}").Result;
                Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
            })
            .QuickCheckThrowOnFailure();
    }
}
