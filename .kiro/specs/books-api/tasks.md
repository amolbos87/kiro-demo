# Implementation Plan: Books API

## Overview

Implement a .NET Core Web API for CRUD operations on a books collection. The application uses Entity Framework Core with SQLite for persistence, the Repository pattern for data-access abstraction, and global exception-handling middleware. Tests cover unit, integration, and property-based scenarios.

---

## Tasks

- [x] 1. Scaffold project structure and add NuGet packages
  - [x] 1.1 Create the ASP.NET Core Web API project and add required NuGet packages
    - Create a new ASP.NET Core Web API project (e.g., `BooksApi`)
    - Add NuGet packages: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design`
    - Add test project(s): `xunit`, `Moq`, `FsCheck.Xunit`, `Microsoft.AspNetCore.Mvc.Testing`
    - Create the folder structure: `Models/`, `DTOs/`, `Data/`, `Repositories/`, `Controllers/`, `Middleware/`
    - _Requirements: 6.1, 6.5_

- [x] 2. Define data models and DTOs
  - [x] 2.1 Create the `Book` entity class
    - Implement `Book` in `Models/Book.cs` with properties: `Id`, `Title` (`[Required][MaxLength(500)]`), `Author` (`[Required][MaxLength(300)]`), `Pages` (`[Range(1,50000)]`), `Language` (`[Required]`), `Genre` (`[Required]`), `CoverImageUrl` (nullable)
    - _Requirements: 6.1_

  - [x] 2.2 Create `CreateBookDto` and `UpdateBookDto`
    - Implement `CreateBookDto` in `DTOs/CreateBookDto.cs` with data annotations matching POST validation rules (Title ≤ 500, Author ≤ 300, Pages 1–50000, required Language & Genre)
    - Implement `UpdateBookDto` in `DTOs/UpdateBookDto.cs` with data annotations matching PUT validation rules (Title ≤ 200, Pages 1–99999, required Author)
    - _Requirements: 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 4.4, 4.5, 4.6, 4.7_

  - [x] 2.3 Write property test for invalid input rejection (Property 2)
    - **Property 2: Invalid input is always rejected**
    - Use FsCheck generators to produce `CreateBookDto` instances that violate at least one constraint (empty Title/Author, Pages < 1 or > 50000, empty Language, empty Genre, Title > 500 chars)
    - Assert that model validation rejects each generated invalid DTO before it reaches the repository
    - Tag test: `// Feature: books-api, Property 2: Invalid input is always rejected`
    - **Validates: Requirements 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8**

- [x] 3. Implement `BookDbContext` and EF Core configuration
  - [x] 3.1 Create `BookDbContext` and configure the Books table
    - Implement `BookDbContext : DbContext` in `Data/BookDbContext.cs` with `DbSet<Book> Books`
    - Override `OnModelCreating` to enforce column constraints (max lengths, NOT NULL, primary key)
    - Register `BookDbContext` as a scoped service in `Program.cs` using the SQLite connection string from `appsettings.json`
    - _Requirements: 6.1, 6.5_

  - [x] 3.2 Create and apply the initial EF Core migration
    - Add the initial migration (`InitialCreate`) via `dotnet ef migrations add`
    - Configure startup code in `Program.cs` to call `dbContext.Database.MigrateAsync()` before the app starts serving requests; log and halt on migration failure
    - _Requirements: 6.2, 6.3_

- [x] 4. Implement the Repository layer
  - [x] 4.1 Define `IBookRepository` interface
    - Create `Repositories/IBookRepository.cs` declaring all five async methods: `GetAllAsync`, `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` with exact signatures from the design
    - _Requirements: 6.4, 7.1_

  - [x] 4.2 Implement `BookRepository`
    - Create `Repositories/BookRepository.cs` implementing `IBookRepository`
    - Use `BookDbContext` (constructor-injected) with EF Core async APIs: `FindAsync`, `ToListAsync`, `AddAsync`, `Remove`, `SaveChangesAsync`
    - Let exceptions propagate (do not swallow them)
    - Register `IBookRepository → BookRepository` as a scoped service in `Program.cs`
    - _Requirements: 6.5, 7.2, 7.3, 7.4_

  - [x] 4.3 Write unit tests for `BookRepository`
    - Use an in-memory SQLite `BookDbContext` (not Moq) to test each repository method
    - Cover: `GetAllAsync` returns all records; `GetByIdAsync` returns null for missing id; `AddAsync` persists and returns with assigned Id; `UpdateAsync` returns null for missing id; `DeleteAsync` returns false for missing id, true on success
    - _Requirements: 6.4, 7.1_

- [ ] 5. Implement `BooksController`
  - [x] 5.1 Implement `GetAll` and `GetById` actions
    - Create `Controllers/BooksController.cs` with `[ApiController][Route("api/books")]`
    - Constructor-inject `IBookRepository`
    - Implement `GetAll`: returns `200 + IEnumerable<Book>`; empty array when none exist
    - Implement `GetById`: returns `200 + Book` or `404` if not found; validate `id > 0` returning `400` for invalid ids
    - _Requirements: 1.1, 1.2, 1.3, 2.1, 2.2, 2.3, 2.4_

  - [x] 5.2 Implement `Create` action
    - Implement `Create` (POST): accepts `CreateBookDto`, maps to `Book`, calls `AddAsync`, returns `201 Created` with `Location` header pointing to the new resource
    - _Requirements: 3.1, 3.9_

  - [x] 5.3 Implement `Update` action
    - Implement `Update` (PUT): accepts `int id` + `UpdateBookDto`, maps to `Book`, calls `UpdateAsync`, returns `200 + updated Book` or `404` if not found
    - _Requirements: 4.1, 4.2, 4.3_

  - [x] 5.4 Implement `Delete` action
    - Implement `Delete` (DELETE): accepts `int id`, calls `DeleteAsync`, returns `204 No Content` or `404` if not found
    - _Requirements: 5.1, 5.2, 5.3_

  - [-] 5.5 Write unit tests for `BooksController`
    - Use `Moq` to mock `IBookRepository`
    - Cover: `GetAll` returns 200 with list; `GetAll` returns 200 with empty array; `GetById` returns 200 for existing id; `GetById` returns 404 for missing id; `GetById` returns 400 for id ≤ 0; `Create` returns 201 with Location header; `Update` returns 200 for existing id; `Update` returns 404 for missing id; `Delete` returns 204 for existing id; `Delete` returns 404 for missing id
    - _Requirements: 1.1, 1.2, 2.1, 2.2, 2.3, 2.4, 3.1, 4.1, 4.2, 5.1, 5.2_

- [~] 6. Checkpoint — Ensure all unit tests pass
  - Ensure all unit tests pass, ask the user if questions arise.

- [ ] 7. Implement global exception-handling middleware
  - [x] 7.1 Create `ExceptionHandlingMiddleware`
    - Create `Middleware/ExceptionHandlingMiddleware.cs`
    - Catch `DbException` (and subtypes) → respond with `503 Service Unavailable` and `{"error": "..."}` body
    - Catch all other unhandled exceptions → respond with `500 Internal Server Error` and `{"error": "..."}` body
    - Log exceptions before writing the response
    - Register the middleware early in the pipeline in `Program.cs`
    - _Requirements: 1.4, 3.10, 6.3, 7.2_

  - [-] 7.2 Write unit tests for `ExceptionHandlingMiddleware`
    - Test that a `DbException` results in a 503 response with the expected JSON error body
    - Test that a generic `Exception` results in a 500 response with the expected JSON error body
    - _Requirements: 1.4, 3.10_

- [ ] 8. Integration tests
  - [-] 8.1 Set up `WebApplicationFactory` test harness
    - Create an integration test project (or class) that uses `WebApplicationFactory<Program>` with an in-memory or test-specific SQLite database
    - Override DI registration to use a test SQLite connection string
    - Ensure migrations run as part of the test host startup
    - _Requirements: 6.2_

  - [~] 8.2 Write integration tests for all five endpoints
    - Happy-path POST → 201, GET list → 200, GET by id → 200, PUT → 200, DELETE → 204
    - GET/PUT/DELETE with non-existent id → 404
    - POST with missing required field → 400; PUT with Title > 200 chars → 400
    - _Requirements: 1.1, 2.1, 2.2, 3.1, 3.2, 4.1, 4.2, 5.1, 5.2_

- [ ] 9. Property-based tests with FsCheck
  - [~] 9.1 Write property test for create-then-retrieve round trip (Property 1)
    - Use FsCheck to generate random valid `CreateBookDto` instances (Title ≤ 500, Author ≤ 300, Pages 1–50000, non-empty Language & Genre)
    - POST via `WebApplicationFactory`, then GET by returned Id; assert all fields match the submitted values exactly
    - Run minimum 100 iterations
    - Tag: `// Feature: books-api, Property 1: Create-then-retrieve round trip`
    - **Validates: Requirements 3.1, 6.1**

  - [~] 9.2 Write property test for update reflects submitted fields (Property 3)
    - Seed a book, generate random valid `UpdateBookDto` via FsCheck, PUT, then GET by Id; assert all fields match submitted values
    - Run minimum 100 iterations
    - Tag: `// Feature: books-api, Property 3: Update reflects submitted fields`
    - **Validates: Requirements 4.1**

  - [~] 9.3 Write property test for delete removes the record (Property 4)
    - Seed a book, DELETE by Id via `WebApplicationFactory`; assert 204, then assert subsequent GET returns 404
    - Run minimum 100 iterations
    - Tag: `// Feature: books-api, Property 4: Delete removes the record`
    - **Validates: Requirements 5.1**

  - [~] 9.4 Write property test for non-existent Id returns 404 (Property 5)
    - Generate random positive integers that do not correspond to any seeded book; assert GET, PUT, and DELETE all return 404
    - Run minimum 100 iterations
    - Tag: `// Feature: books-api, Property 5: Non-existent Id always returns 404`
    - **Validates: Requirements 2.2, 4.2, 5.2**

- [~] 10. Final checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

---

## Notes

- Tasks marked with `*` are optional and can be skipped for a faster MVP build
- Each task references specific requirements for traceability
- Checkpoints at steps 6 and 10 validate incremental correctness
- Property tests use FsCheck with a minimum of 100 iterations per property
- Unit tests use Moq for controller-layer isolation; repository unit tests use a real in-memory SQLite DbContext
- Integration tests use `WebApplicationFactory<Program>` with a test SQLite database

---

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["2.1", "3.1"] },
    { "id": 2, "tasks": ["2.2", "3.2", "4.1"] },
    { "id": 3, "tasks": ["2.3", "4.2"] },
    { "id": 4, "tasks": ["4.3", "5.1", "7.1"] },
    { "id": 5, "tasks": ["5.2", "5.3", "5.4"] },
    { "id": 6, "tasks": ["5.5", "7.2", "8.1"] },
    { "id": 7, "tasks": ["8.2", "9.1", "9.2", "9.3", "9.4"] }
  ]
}
```
