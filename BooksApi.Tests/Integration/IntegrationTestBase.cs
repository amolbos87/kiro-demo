using System;
using BooksApi.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BooksApi.Tests.Integration;

/// <summary>
/// Base class for integration tests using WebApplicationFactory with a test SQLite database.
/// Creates an in-memory SQLite database and overrides DI registration to use test connection.
/// Migrations run automatically during test host startup.
/// </summary>
public class IntegrationTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly SqliteConnection _connection;
    protected readonly BookDbContext _testDbContext;

    public IntegrationTestBase()
    {
        // Create a new in-memory SQLite connection
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Create WebApplicationFactory with custom configuration
        _factory = new CustomWebApplicationFactory(_connection);

        // Get the test database context for seeding and verification
        _testDbContext = _factory.Services.GetRequiredService<BookDbContext>();
    }

    private class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection;

        public CustomWebApplicationFactory(SqliteConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Override DI registration to use test SQLite connection instead of default connection string.
        /// This ensures migrations run on the test database during startup.
        /// </summary>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(DbContextOptions<BookDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Register DbContext with test SQLite connection
                services.AddDbContext<BookDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });
            });

            // Ensure migrations are applied during test host startup
            builder.UseSetting("ConnectionStrings:DefaultConnection", _connection.ConnectionString);
        }
    }

    public void Dispose()
    {
        _testDbContext.Dispose();
        _factory.Dispose();
        _connection.Dispose();
    }
}
