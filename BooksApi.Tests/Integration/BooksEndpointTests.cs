using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BooksApi.DTOs;
using BooksApi.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BooksApi.Tests.Integration;

/// <summary>
/// Integration tests for Books API endpoints using WebApplicationFactory.
/// Tests cover GetAll, GetById, Create, Update, Delete operations.
/// </summary>
public class BooksEndpointTests : IClassFixture<IntegrationTestBase>
{
    private readonly IntegrationTestBase _fixture;

    public BooksEndpointTests(IntegrationTestBase fixture)
    {
        _fixture = fixture;
    }

    private HttpClient CreateClient() => _fixture._factory.CreateClient();

    // ==================== GET ALL TESTS ====================

    [Fact]
    public async Task GetAll_WhenNoBooksExist_ReturnsEmptyArray()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/books");
        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        Assert.NotNull(books);
        Assert.Empty(books);
    }

    [Fact]
    public async Task GetAll_WhenBooksExist_ReturnsAllBooks()
    {
        // Arrange
        var client = CreateClient();
        var book1 = new Book { Title = "Book 1", Author = "Author 1", Pages = 100, Language = "English", Genre = "Fiction" };
        var book2 = new Book { Title = "Book 2", Author = "Author 2", Pages = 200, Language = "Spanish", Genre = "Non-Fiction" };

        await _fixture._testDbContext.Books.AddAsync(book1);
        await _fixture._testDbContext.Books.AddAsync(book2);
        await _fixture._testDbContext.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/api/books");
        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        Assert.NotNull(books);
        Assert.Equal(2, books.Count);
    }

    // ==================== GET BY ID TESTS ====================

    [Fact]
    public async Task GetById_WithValidId_ReturnsBook()
    {
        // Arrange
        var client = CreateClient();
        var book = new Book { Title = "Test Book", Author = "Test Author", Pages = 150, Language = "English", Genre = "Fiction" };
        await _fixture._testDbContext.Books.AddAsync(book);
        await _fixture._testDbContext.SaveChangesAsync();

        // Act
        var response = await client.GetAsync($"/api/books/{book.Id}");
        response.EnsureSuccessStatusCode();

        var returnedBook = await response.Content.ReadFromJsonAsync<Book>();

        // Assert
        Assert.NotNull(returnedBook);
        Assert.Equal(book.Id, returnedBook.Id);
        Assert.Equal(book.Title, returnedBook.Title);
        Assert.Equal(book.Author, returnedBook.Author);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();
        var nonExistentId = 9999;

        // Act
        var response = await client.GetAsync($"/api/books/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithInvalidIdZero_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/books/0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithInvalidIdNegative_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/books/-1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ==================== CREATE TESTS ====================

    [Fact]
    public async Task Create_WithValidData_CreatesBookAndReturns201()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateBookDto
        {
            Title = "New Book",
            Author = "New Author",
            Pages = 300,
            Language = "English",
            Genre = "Fiction"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/books", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdBook = await response.Content.ReadFromJsonAsync<Book>();
        Assert.NotNull(createdBook);
        Assert.NotEqual(0, createdBook.Id);
        Assert.Equal(createDto.Title, createdBook.Title);
        Assert.Equal(createDto.Author, createdBook.Author);
    }

    [Fact]
    public async Task Create_WithMissingRequiredField_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateBookDto
        {
            Title = "Book Title",
            Author = "Author Name",
            Pages = 100,
            Language = "English",
            // Genre is missing
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/books", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidTitleLength_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateBookDto
        {
            Title = new string('A', 501), // Exceeds 500 char limit
            Author = "Author",
            Pages = 100,
            Language = "English",
            Genre = "Fiction"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/books", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidPages_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateBookDto
        {
            Title = "Book",
            Author = "Author",
            Pages = 0, // Invalid: must be >= 1
            Language = "English",
            Genre = "Fiction"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/books", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithCoverImageUrl_NullAllowed()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateBookDto
        {
            Title = "Book With No Cover",
            Author = "Author",
            Pages = 100,
            Language = "English",
            Genre = "Fiction",
            CoverImageUrl = null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/books", createDto);
        response.EnsureSuccessStatusCode();

        var createdBook = await response.Content.ReadFromJsonAsync<Book>();

        // Assert
        Assert.NotNull(createdBook);
        Assert.Null(createdBook.CoverImageUrl);
    }

    // ==================== UPDATE TESTS ====================

    [Fact]
    public async Task Update_WithValidId_UpdatesBook()
    {
        // Arrange
        var client = CreateClient();
        var book = new Book { Title = "Original Title", Author = "Original Author", Pages = 100, Language = "English", Genre = "Fiction" };
        await _fixture._testDbContext.Books.AddAsync(book);
        await _fixture._testDbContext.SaveChangesAsync();

        var updateDto = new UpdateBookDto
        {
            Title = "Updated Title",
            Author = "Updated Author",
            Pages = 200,
            Language = "Spanish",
            Genre = "Non-Fiction"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/books/{book.Id}", updateDto);
        response.EnsureSuccessStatusCode();

        var updatedBook = await response.Content.ReadFromJsonAsync<Book>();

        // Assert
        Assert.NotNull(updatedBook);
        Assert.Equal(book.Id, updatedBook.Id);
        Assert.Equal(updateDto.Title, updatedBook.Title);
        Assert.Equal(updateDto.Author, updatedBook.Author);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();
        var nonExistentId = 9999;
        var updateDto = new UpdateBookDto
        {
            Title = "Updated Title",
            Author = "Updated Author",
            Pages = 200,
            Language = "Spanish",
            Genre = "Non-Fiction"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/books/{nonExistentId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithInvalidTitleLength_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var book = new Book { Title = "Original", Author = "Author", Pages = 100, Language = "English", Genre = "Fiction" };
        await _fixture._testDbContext.Books.AddAsync(book);
        await _fixture._testDbContext.SaveChangesAsync();

        var updateDto = new UpdateBookDto
        {
            Title = new string('B', 201), // Exceeds 200 char limit for Update
            Author = "Updated Author",
            Pages = 200,
            Language = "Spanish",
            Genre = "Non-Fiction"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/books/{book.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithMissingAuthor_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var book = new Book { Title = "Original", Author = "Author", Pages = 100, Language = "English", Genre = "Fiction" };
        await _fixture._testDbContext.Books.AddAsync(book);
        await _fixture._testDbContext.SaveChangesAsync();

        var updateDto = new UpdateBookDto
        {
            Title = "Updated Title",
            Author = "", // Empty author
            Pages = 200,
            Language = "Spanish",
            Genre = "Non-Fiction"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/books/{book.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithInvalidIdZero_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var updateDto = new UpdateBookDto
        {
            Title = "Updated Title",
            Author = "Author",
            Pages = 200,
            Language = "Spanish",
            Genre = "Non-Fiction"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/books/0", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ==================== DELETE TESTS ====================

    [Fact]
    public async Task Delete_WithValidId_DeletesBookAndReturns204()
    {
        // Arrange
        var client = CreateClient();
        var book = new Book { Title = "To Be Deleted", Author = "Author", Pages = 100, Language = "English", Genre = "Fiction" };
        await _fixture._testDbContext.Books.AddAsync(book);
        await _fixture._testDbContext.SaveChangesAsync();

        // Act
        var response = await client.DeleteAsync($"/api/books/{book.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the book was deleted
        var getResponse = await client.GetAsync($"/api/books/{book.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();
        var nonExistentId = 9999;

        // Act
        var response = await client.DeleteAsync($"/api/books/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithInvalidIdZero_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/books/0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithInvalidIdNegative_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/books/-1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ==================== COMPREHENSIVE TESTS ====================

    [Fact]
    public async Task FullWorkflow_CreateGetUpdateDelete()
    {
        // Arrange
        var client = CreateClient();

        // Create
        var createDto = new CreateBookDto
        {
            Title = "Workflow Test",
            Author = "Workflow Author",
            Pages = 150,
            Language = "English",
            Genre = "Test"
        };

        var createResponse = await client.PostAsJsonAsync("/api/books", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createdBook = await response.Content.ReadFromJsonAsync<Book>();

        // Get by ID
        var getIdResponse = await client.GetAsync($"/api/books/{createdBook.Id}");
        getIdResponse.EnsureSuccessStatusCode();
        var retrievedBook = await getIdResponse.Content.ReadFromJsonAsync<Book>();
        Assert.Equal(createdBook.Id, retrievedBook.Id);

        // Update
        var updateDto = new UpdateBookDto
        {
            Title = "Workflow Test Updated",
            Author = "Workflow Author Updated",
            Pages = 200,
            Language = "Spanish",
            Genre = "Updated"
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/books/{createdBook.Id}", updateDto);
        updateResponse.EnsureSuccessStatusCode();
        var updatedBook = await updateResponse.Content.ReadFromJsonAsync<Book>();
        Assert.Equal(updateDto.Title, updatedBook.Title);

        // Delete
        var deleteResponse = await client.DeleteAsync($"/api/books/{createdBook.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify deletion
        var verifyResponse = await client.GetAsync($"/api/books/{createdBook.Id}");
        Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
    }
}
