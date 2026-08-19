# Project Structure: Books API

## Overview

The project follows a layered architecture with clear separation of concerns. The main application (`BooksApi`) contains all production code, while tests are in a separate `BooksApi.Tests` project.

## Folder Structure

```
BooksApi/
├── Controllers/          # API endpoints (HTTP layer)
├── Data/                 # Database context and EF Core configuration
├── DTOs/                 # Data Transfer Objects for request/response validation
├── Middleware/           # Custom middleware (e.g., exception handling)
├── Models/               # Domain entities (database models)
├── Repositories/         # Data access abstraction layer
├── Migrations/           # EF Core database migrations
├── Tests/                # Test files (if using inline tests)
├── Program.cs            # Application entry point and DI configuration
├── appsettings.json      # Configuration (connection strings, logging)
├── WeatherForecast.cs    # Template file (can be removed)
└── BooksApi.csproj       # Project file with dependencies
```

## File Responsibilities

### Controllers (`Controllers/`)

- Define API endpoints using `[ApiController]` and `[Route]` attributes
- Accept DTOs as action parameters for validation
- Return HTTP status codes and results via `ActionResult<T>`
- Inject dependencies via constructor injection
- Handle database exceptions via try-catch or middleware

**Example:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _repository;
    
    public BooksController(IBookRepository repository)
    {
        _repository = repository;
    }
}
```

### Data (`Data/`)

- Contains `DbContext` classes for EF Core
- Configure model relationships and constraints in `OnModelCreating`
- Register DbContext with DI container in `Program.cs`

### DTOs (`DTOs/`)

- `CreateBookDto`: For POST requests with validation attributes
- `UpdateBookDto`: For PUT requests with validation attributes
- Use data annotations for validation:
  - `[Required]` for mandatory fields
  - `[MaxLength(n)]` for string length
  - `[Range(min, max)]` for numeric ranges

### Middleware (`Middleware/`)

- Custom middleware components for cross-cutting concerns
- Exception handling middleware with specific handling for `DbException` → 503, others → 500
- Register middleware in `Program.cs` using extension methods

### Models (`Models/`)

- Domain entities representing database tables
- Properties decorated with validation attributes
- `Book` entity has: Id, Title, Author, Pages, Language, Genre, CoverImageUrl

### Repositories (`Repositories/`)

- Define interfaces (`IBookRepository`) with CRUD method signatures
- Implement concrete classes (`BookRepository`) using EF Core
- Use async methods: `GetAllAsync`, `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`
- Register as scoped services: `services.AddScoped<IBookRepository, BookRepository>()`

## Test Project Structure

```
BooksApi.Tests/
├── Controllers/          # Unit tests for controllers (using Moq)
├── Integration/          # Integration tests (using WebApplicationFactory)
├── Middleware/           # Unit tests for middleware components
├── Repositories/         # Unit tests for repository layer (real DbContext)
├── GlobalUsings.cs       # Global xunit imports and feature tags
├── BooksApi.Tests.csproj # Test project with xunit, Moq, FsCheck
└── <test files>          # Tests tagged with feature properties
```

## Test Organization

Tests are organized by layer and tagged with feature properties:

- **Unit tests**: Mock repository, test controller logic
- **Integration tests**: Real database, test full request pipeline
- **Property-based tests**: FsCheck generators for validation properties

Feature tags in tests:
```csharp
// Feature: books-api, Property 1: Create-then-retrieve round trip
// Feature: books-api, Property 2: Invalid input is always rejected
// Feature: books-api, Property 3: Update reflects submitted fields
// Feature: books-api, Property 4: Delete removes the record
// Feature: books-api, Property 5: Non-existent Id always returns 404
```

## Dependency Injection Registration

In `Program.cs`:
```csharp
builder.Services.AddControllers();
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

## Configuration

- Connection string stored in `appsettings.json` under `ConnectionStrings:DefaultConnection`
- SQLite database file: `books.db` (default location)
- Logging configured for Development (Information) and Production (Warning for ASP.NET Core)