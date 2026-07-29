using BooksApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BooksApi.Tests.Controllers;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly ExceptionHandlingMiddleware _middleware;

    public ExceptionHandlingMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_DbException_Returns503Response()
    {
        // Arrange
        var expectedErrorMessage = "Service temporarily unavailable. Database connection failed.";
        _nextMock.Setup(d => d(It.IsAny<HttpContext>()))
            .ThrowsAsync(new DbUpdateException("Database error"));

        var httpContext = new DefaultHttpContext();
        
        // Act
        await _middleware.InvokeAsync(httpContext);
        
        // Assert
        Assert.Equal(503, httpContext.Response.StatusCode);
        
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        
        Assert.Contains("\"error\"", responseBody);
        Assert.Contains(expectedErrorMessage, responseBody);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500Response()
    {
        // Arrange
        var expectedErrorMessage = "An unexpected error occurred. Please try again later.";
        _nextMock.Setup(d => d(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Generic error"));

        var httpContext = new DefaultHttpContext();
        
        // Act
        await _middleware.InvokeAsync(httpContext);
        
        // Assert
        Assert.Equal(500, httpContext.Response.StatusCode);
        
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        
        Assert.Contains("\"error\"", responseBody);
        Assert.Contains(expectedErrorMessage, responseBody);
    }

    [Fact]
    public async Task InvokeAsync_DbException_LoggingOccurs()
    {
        // Arrange
        var exception = new DbUpdateException("Database error");
        _nextMock.Setup(d => d(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        var httpContext = new DefaultHttpContext();
        
        // Act
        await _middleware.InvokeAsync(httpContext);
        
        // Assert
        _loggerMock.Verify(
            l => l.LogError(It.IsAny<LogLevel>(), exception, "Database exception occurred."),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_LoggingOccurs()
    {
        // Arrange
        var exception = new InvalidOperationException("Generic error");
        _nextMock.Setup(d => d(It.IsAny<HttpContext>()))
            .ThrowsAsync(exception);

        var httpContext = new DefaultHttpContext();
        
        // Act
        await _middleware.InvokeAsync(httpContext);
        
        // Assert
        _loggerMock.Verify(
            l => l.LogError(It.IsAny<LogLevel>(), exception, "Unexpected error occurred."),
            Times.Once);
    }
}