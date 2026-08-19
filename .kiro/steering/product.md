# Product: Books API

A RESTful web API built with .NET 8 for managing a collection of books. The API provides full CRUD (Create, Read, Update, Delete) operations for book entities with metadata including Title, Author, Pages, Language, Genre, and optional CoverImageUrl.

## Purpose

The Books API serves as a backend service for clients that need to manage book data with persistent storage using SQLite. It demonstrates a clean architecture with separation of concerns between controllers, repositories, and data access layers.

## Core Features

- **Book Management**: Full CRUD operations for books via RESTful endpoints
- **Data Persistence**: SQLite database with Entity Framework Core migrations
- **Validation**: Input validation using data annotations on DTOs
- **Error Handling**: Global exception middleware for database and unexpected errors
- **Testing**: Comprehensive unit and integration test coverage