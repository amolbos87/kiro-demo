using BooksApi.Data;
using BooksApi.Models;
using BooksApi.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Tests.Repositories;

public class BookRepositoryTests
{
    private BookDbContext CreateInMemoryDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<BookDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new BookDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRecords()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new BookRepository(context);

        var book1 = new Book { Title = "Book 1", Author = "Author 1", Pages = 100, Language = "English", Genre = "Fiction" };
        var book2 = new Book { Title = "Book 2", Author = "Author 2", Pages = 200, Language = "Spanish", Genre = "Non-Fiction" };
        var book3 = new Book { Title = "Book 3", Author = "Author 3", Pages = 300, Language = "French", Genre = "Science" };

        context.Books.Add(book1);
        context.Books.Add(book2);
        context.Books.Add(book3);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        var books = result.ToList();
        Assert.Equal(3, books.Count);
        Assert.Contains(books, b => b.Title == "Book 1");
        Assert.Contains(books, b => b.Title == "Book 2");
        Assert.Contains(books, b => b.Title == "Book 3");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyWhenNoRecords()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsBookForExistingId()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new BookRepository(context);

        var book = new Book { Title = "Test Book", Author = "Test Author", Pages = 150, Language = "English", Genre = "Test" };
        context.Books.Add(book);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(book.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(book.Id, result?.Id);
        Assert.Equal("Test Book", result?.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForMissingId()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_PersistsAndReturnsWithAssignedId()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new BookRepository(context);

        var book = new Book { Title = "New Book", Author = "New Author", Pages = 250, Language = "English", Genre = "Drama" };

        // Act
        var result = await repository.AddAsync(book);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New Book", result.Title);
        Assert.Equal("New Author", result.Author);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullForMissingId()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new BookRepository(context);

        var book = new Book { Id = 999, Title = "Non-existent", Author = "Author", Pages = 100, Language = "English", Genre = "Fiction" };

        // Act
        var result = await repository.UpdateAsync(999, book);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsUpdatedBook()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new BookRepository(context);

        var book = new Book { Title = "Original Title", Author = "Original Author", Pages = 100, Language = "English", Genre = "Fiction" };
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var updatedBook = new Book { Title = "Updated Title", Author = "Updated Author", Pages = 200, Language = "Spanish", Genre = "Non-Fiction" };

        // Act
        var result = await repository.UpdateAsync(book.Id, updatedBook);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(book.Id, result?.Id);
        Assert.Equal("Updated Title", result?.Title);
        Assert.Equal("Updated Author", result?.Author);
        Assert.Equal(200, result?.Pages);
        Assert.Equal("Spanish", result?.Language);
        Assert.Equal("Non-Fiction", result?.Genre);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalseForMissingId()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new BookRepository(context);

        // Act
        var result = await repository.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrueOnSuccess()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new BookRepository(context);

        var book = new Book { Title = "To Delete", Author = "Author", Pages = 100, Language = "English", Genre = "Fiction" };
        context.Books.Add(book);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.DeleteAsync(book.Id);

        // Assert
        Assert.True(result);

        // Verify the book is deleted
        var deletedBook = await repository.GetByIdAsync(book.Id);
        Assert.Null(deletedBook);
    }
}
