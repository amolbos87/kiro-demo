# Requirements Document

## Introduction

The Books API is a .NET Core Web API application that provides CRUD (Create, Read, Update, Delete) operations for managing a collection of books. The API uses Entity Framework Core with SQLite for data persistence and follows the Repository pattern to decouple data access logic from business logic. Consumers of the API can manage books identified by a unique Id, with metadata including Title, Author, Pages, Language, Genre, and CoverImageUrl.

## Glossary

- **API**: The Books Web API application built with .NET Core.
- **Book**: The core domain entity with fields: Id, Title, Author, Pages, Language, Genre, and CoverImageUrl.
- **Repository**: The data access abstraction layer that mediates between the domain and data mapping layers.
- **DbContext**: The Entity Framework Core context responsible for database interactions with SQLite.
- **Client**: Any consumer of the Books API (e.g., front-end application, test tool, or integration).
- **Id**: A unique integer identifier assigned to each Book by the database.

---

## Requirements

### Requirement 1: Retrieve All Books (refined)

**User Story:** As a client, I want to retrieve the full list of books, so that I can display or process the entire book collection.

#### Acceptance Criteria

1. WHEN the Client sends a GET request to `/api/books`, THE API SHALL return an HTTP 200 response containing a JSON array of all Book records stored in the database.
2. WHEN no books exist in the database, THE API SHALL return an HTTP 200 response containing an empty JSON array.
3. THE API SHALL include all Book fields (Id, Title, Author, Pages, Language, Genre, CoverImageUrl) in each element of the returned array.
4. IF the database is unavailable when the Client sends a GET request to `/api/books`, THEN THE API SHALL return an HTTP 503 response containing an error message indicating the service is temporarily unavailable.

---

### Requirement 2: Retrieve a Book by Id (refined)

**User Story:** As a client, I want to retrieve a single book by its unique identifier, so that I can view the details of a specific book.

#### Acceptance Criteria

1. WHEN the Client sends a GET request to `/api/books/{id}` with a valid Id, THE API SHALL return an HTTP 200 response containing the JSON representation of the matching Book including all Book fields (Id, Title, Author, Pages, Language, Genre, CoverImageUrl).
2. IF no Book with the given Id exists when the Client sends a GET request to `/api/books/{id}`, THEN THE API SHALL return an HTTP 404 response with an error message indicating the Book with the given Id was not found.
3. IF the Client sends a GET request to `/api/books/{id}` with a non-integer Id value, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Id must be a positive integer.
4. IF the Client sends a GET request to `/api/books/{id}` with a zero or negative integer Id value, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Id must be a positive integer.

---

### Requirement 3: Add a New Book (refined)

**User Story:** As a client, I want to add a new book to the collection, so that the catalog can be expanded with new entries.

#### Acceptance Criteria

1. WHEN the Client sends a POST request to `/api/books` with a valid JSON body containing Title, Author, Pages, Language, Genre, and CoverImageUrl, THE API SHALL persist the new Book to the database and return an HTTP 201 response containing the created Book including its assigned Id.
2. IF the Client sends a POST request to `/api/books` with a missing or empty Title field, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Title field is required, without persisting any data.
3. THE Title field SHALL be a non-empty string of at most 500 characters; IF the value exceeds 500 characters, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Title length limit.
4. IF the Client sends a POST request to `/api/books` with a missing or empty Author field, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Author field is required, without persisting any data.
5. THE Author field SHALL be a non-empty string of at most 300 characters; IF the value exceeds 300 characters, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Author length limit.
6. IF the Client sends a POST request to `/api/books` with a Pages value less than 1 or greater than 50000, THEN THE API SHALL return an HTTP 400 response with an error message indicating Pages must be between 1 and 50000, without persisting any data.
7. IF the Client sends a POST request to `/api/books` with a missing or empty Language field, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Language field is required, without persisting any data.
8. IF the Client sends a POST request to `/api/books` with a missing or empty Genre field, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Genre field is required, without persisting any data.
9. WHERE CoverImageUrl is omitted in the request body, THE API SHALL persist the Book with a null CoverImageUrl value and return an HTTP 201 response.
10. IF the database is unavailable when the Client sends a POST request to `/api/books`, THEN THE API SHALL return an HTTP 503 response with an error message indicating the service is temporarily unavailable, without persisting any partial data.

---

### Requirement 4: Update an Existing Book (refined)

**User Story:** As a client, I want to update the details of an existing book, so that incorrect or outdated information can be corrected.

#### Acceptance Criteria

1. WHEN the Client sends a PUT request to `/api/books/{id}` with a valid Id and a valid JSON body, THE API SHALL update the corresponding Book in the database and return an HTTP 200 response containing the updated Book with all submitted fields reflected in the response body.
2. IF no Book with the given Id exists when the Client sends a PUT request to `/api/books/{id}`, THEN THE API SHALL return an HTTP 404 response with an error message indicating the Book was not found, without modifying any Book.
3. IF the Client sends a PUT request to `/api/books/{id}` with a malformed or invalid JSON body, THEN THE API SHALL return an HTTP 400 response with an error message indicating the request body is invalid, without modifying any Book.
4. IF the Client sends a PUT request to `/api/books/{id}` with a missing or empty Title field, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Title field is required, without modifying any Book.
5. THE Title field SHALL be a non-empty string of at most 200 characters; IF the value exceeds 200 characters, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Title length limit, without modifying any Book.
6. IF the Client sends a PUT request to `/api/books/{id}` with a missing or empty Author field, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Author field is required, without modifying any Book.
7. IF the Client sends a PUT request to `/api/books/{id}` with a Pages value less than 1 or greater than 99999, THEN THE API SHALL return an HTTP 400 response with an error message indicating Pages must be between 1 and 99999, without modifying any Book.

---

### Requirement 5: Delete a Book (refined)

**User Story:** As a client, I want to delete a book from the collection, so that outdated or erroneous entries can be removed.

#### Acceptance Criteria

1. WHEN the Client sends a DELETE request to `/api/books/{id}` with a valid Id, THE API SHALL remove the corresponding Book from the database and return an HTTP 204 response with no body.
2. IF no Book with the given Id exists when the Client sends a DELETE request to `/api/books/{id}`, THEN THE API SHALL return an HTTP 404 response with an error message indicating the Book with the given Id was not found.
3. IF the Client sends a DELETE request to `/api/books/{id}` with a malformed or non-integer Id value, THEN THE API SHALL return an HTTP 400 response with an error message indicating the Id is invalid.

---

### Requirement 6: Data Persistence with Entity Framework Core and SQLite (refined)

**User Story:** As a developer, I want the API to persist data using Entity Framework Core with a SQLite database, so that book data survives application restarts.

#### Acceptance Criteria

1. THE DbContext SHALL manage the Book entity and map it to a SQLite database table named `Books`, with at minimum: an auto-incremented integer primary key (Id), non-null string columns for Title and Author (max 255 characters each), an integer column for Pages, and nullable string columns for Language, Genre, and CoverImageUrl.
2. WHEN the application starts, THE API SHALL apply any pending Entity Framework Core migrations before the first request is handled to ensure the database schema is up to date.
3. IF an Entity Framework Core migration fails at startup, THEN THE API SHALL log the error and halt startup rather than serving requests against an inconsistent schema.
4. THE Repository interface SHALL define the CRUD contract exposing at minimum the following five operations: create a Book, retrieve all Books, retrieve a Book by Id, update a Book, and delete a Book by Id.
5. THE API SHALL register the Repository as a scoped service and the DbContext as a scoped service with the dependency injection container so that controllers receive them via constructor injection.

---

### Requirement 7: Repository Pattern (refined)

**User Story:** As a developer, I want a Repository abstraction over the data layer, so that business logic and controllers are decoupled from direct database access.

#### Acceptance Criteria

1. THE IBookRepository interface SHALL declare the following async methods: GetAllAsync (returns IEnumerable<Book>), GetByIdAsync (accepts int id, returns Book or null), AddAsync (accepts Book, returns created Book), UpdateAsync (accepts int id and Book, returns updated Book or null), and DeleteAsync (accepts int id, returns bool indicating whether a record was deleted).
2. WHEN a Repository operation encounters a database error, THE Repository SHALL allow the exception to propagate to the calling layer so that the API controller can return an appropriate HTTP 500 response.
3. THE API controllers SHALL depend only on IBookRepository interface, not on the concrete BookRepository implementation or the BookDbContext directly.
4. THE concrete BookRepository SHALL implement IBookRepository and use the injected BookDbContext to execute all data operations.
