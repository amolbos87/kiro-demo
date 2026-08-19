# Technology Stack: Books API

## Framework & Runtime

- **Target Framework**: .NET 8.0 (net8.0)
- **Project SDK**: Microsoft.NET.Sdk.Web
- **Language**: C# with nullable reference types enabled
- **Implicit Usings**: Enabled

## Core Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.EntityFrameworkCore | 8.0.0 | EF Core ORM |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.0 | SQLite database provider |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 | EF Core tooling (migrations) |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 | Design-time services for migrations |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.0 | Integration testing |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.0 | WebApplicationFactory for integration tests |

## Testing Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| xunit | 2.9.0 | Unit testing framework |
| Moq | 4.20.70 | Mocking library for unit tests |
| FsCheck.Xunit | 2.16.4 | Property-based testing |
| Microsoft.EntityFrameworkCore.InMemory | 8.0.0 | In-memory database for tests |
| Microsoft.Data.Sqlite | 8.0.0 | SQLite ADO.NET provider |

## Build & Run Commands

### Restore Dependencies
```powershell
dotnet restore
```

### Build Project
```powershell
dotnet build
```

### Run Application
```powershell
dotnet run --project BooksApi
```

### Run Tests
```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test BooksApi.Tests

# Run with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```

### EF Core Migrations

```powershell
# Add a new migration
dotnet ef migrations add <MigrationName> --project BooksApi --startup-project BooksApi

# Update database
dotnet ef database update --project BooksApi --startup-project BooksApi

# Remove last migration
dotnet ef migrations remove --project BooksApi --startup-project BooksApi
```

## Code Style

- **Nullable reference types**: Enabled (`<Nullable>enable</Nullable>`)
- **Implicit usings**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- **File-scoped namespaces**: Used throughout
- **Async/await**: Used for all I/O operations (database, HTTP)
- **Exception handling**: Try-catch in middleware layer with specific handling for DbException