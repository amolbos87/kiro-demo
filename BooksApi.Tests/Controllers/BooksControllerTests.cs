using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BooksApi.Controllers;
using BooksApi.DTOs;
using BooksApi.Models;
using BooksApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BooksApi.Tests.Controllers;

public class BooksControllerTests
{
    private readonly Mock<IBookRepository> _mockRepository;
    private readonly BooksController _controller;

    public BooksControllerTests()
    {
        _mockRepository = new Mock<IBookRepository>();
        _controller = new BooksController(_mockRepository.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithBookList()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = 1, Title = "Book 1", Author = "Author 1", Pages = 100, Language = "English", Genre = "Fiction" },
            new Book { Id = 2, Title = "Book 2", Author = "Author 2", Pages = 200, Language = "Spanish", Genre = "Non-Fiction" }
        };
        _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync((IEnumerable<Book>)books);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedBooks = Assert.IsType<List<Book>>(okResult.Value);
        Assert.Equal(2, returnedBooks.Count);
        Assert.Equal(books[0].Title, returnedBooks[0].Title);
        Assert.Equal(books[1].Title, returnedBooks[1].Title);
        _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithEmptyList()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Book>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedBooks = Assert.IsType<List<Book>>(okResult.Value);
        Assert.Empty(returnedBooks);
        _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ReturnsOkResult_ForExistingId()
    {
        // Arrange
        var book = new Book { Id = 5, Title = "Test Book", Author = "Test Author", Pages = 150, Language = "English", Genre = "Fiction" };
        _mockRepository.Setup(repo => repo.GetByIdAsync(5)).ReturnsAsync(book);

        // Act
        var result = await _controller.GetById(5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedBook = Assert.IsType<Book>(okResult.Value);
        Assert.Equal(5, returnedBook.Id);
        Assert.Equal("Test Book", returnedBook.Title);
        _mockRepository.Verify(repo => repo.GetByIdAsync(5), Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForMissingId()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetByIdAsync(10)).ReturnsAsync((Book?)null);

        // Act
        var result = await _controller.GetById(10);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        _mockRepository.Verify(repo => repo.GetByIdAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsBadRequest_ForIdLessThanOrEqualToZero()
    {
        // Act
        var resultNegative = await _controller.GetById(-1);
        var resultZero = await _controller.GetById(0);

        // Assert
        var badRequestNegative = Assert.IsType<BadRequestObjectResult>(resultNegative.Result);
        var badRequestZero = Assert.IsType<BadRequestObjectResult>(resultZero.Result);
        Assert.Equal(400, badRequestNegative.StatusCode);
        Assert.Equal(400, badRequestZero.StatusCode);
        _mockRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithLocationHeader()
    {
        // Arrange
        var dto = new CreateBookDto
        {
            Title = "New Book",
            Author = "New Author",
            Pages = 300,
            Language = "English",
            Genre = "Fiction"
        };
        var createdBook = new Book
        {
            Id = 1,
            Title = "New Book",
            Author = "New Author",
            Pages = 300,
            Language = "English",
            Genre = "Fiction"
        };
        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Book>())).ReturnsAsync(createdBook);
        _controller.ModelState.Clear();

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(BooksController.GetById), createdAtActionResult.ActionName);
        Assert.Equal("id", createdAtActionResult.RouteValues["id"]);
        Assert.Equal(1, createdAtActionResult.RouteValues["id"]);
        Assert.Equal(201, createdAtActionResult.StatusCode);
        var returnedBook = Assert.IsType<Book>(createdAtActionResult.Value);
        Assert.Equal(1, returnedBook.Id);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var dto = new CreateBookDto
        {
            Title = "",
            Author = "Author",
            Pages = 100,
            Language = "English",
            Genre = "Fiction"
        };
        _controller.ModelState.AddModelError("Title", "Title is required.");

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Never);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ReturnsOkResult_ForExistingId()
    {
        // Arrange
        var dto = new UpdateBookDto
        {
            Title = "Updated Book",
            Author = "Updated Author",
            Pages = 250,
            Language = "English",
            Genre = "Fiction"
        };
        var updatedBook = new Book
        {
            Id = 5,
            Title = "Updated Book",
            Author = "Updated Author",
            Pages = 250,
            Language = "English",
            Genre = "Fiction"
        };
        _mockRepository.Setup(repo => repo.UpdateAsync(5, It.IsAny<Book>())).ReturnsAsync(updatedBook);
        _controller.ModelState.Clear();

        // Act
        var result = await _controller.Update(5, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedBook = Assert.IsType<Book>(okResult.Value);
        Assert.Equal(5, returnedBook.Id);
        Assert.Equal("Updated Book", returnedBook.Title);
        _mockRepository.Verify(repo => repo.UpdateAsync(5, It.IsAny<Book>()), Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_ForMissingId()
    {
        // Arrange
        var dto = new UpdateBookDto
        {
            Title = "Updated Book",
            Author = "Updated Author",
            Pages = 250,
            Language = "English",
            Genre = "Fiction"
        };
        _mockRepository.Setup(repo => repo.UpdateAsync(10, It.IsAny<Book>())).ReturnsAsync((Book?)null);
        _controller.ModelState.Clear();

        // Act
        var result = await _controller.Update(10, dto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        _mockRepository.Verify(repo => repo.UpdateAsync(10, It.IsAny<Book>()), Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ReturnsNoContent_ForExistingId()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.DeleteAsync(5)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(5);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
        _mockRepository.Verify(repo => repo.DeleteAsync(5), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_ForMissingId()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.DeleteAsync(10)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(10);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        _mockRepository.Verify(repo => repo.DeleteAsync(10), Times.Once);
    }

    #endregion
}
