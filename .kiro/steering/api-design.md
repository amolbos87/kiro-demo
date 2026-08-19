# API Design: Books API

## Base URL

All endpoints are prefixed with `/api/books`.

## Endpoints

### GET /api/books

Retrieve all books.

**Request**
- Method: `GET`
- Path: `/api/books`

**Response**
- `200 OK`: Returns JSON array of all books
- `503 Service Unavailable`: Database connection failed

**Example Response:**
```json
[
  {
    "id": 1,
    "title": "The Great Gatsby",
    "author": "F. Scott Fitzgerald",
    "pages": 180,
    "language": "English",
    "genre": "Fiction",
    "coverImageUrl": "https://example.com/cover1.jpg"
  }
]
```

---

### GET /api/books/{id}

Retrieve a specific book by ID.

**Request**
- Method: `GET`
- Path: `/api/books/{id}`
- Route constraint: `id` must be a positive integer (`{id:int}`)

**Response**
- `200 OK`: Returns the requested book
- `400 Bad Request`: Invalid ID (zero, negative, or non-integer)
- `404 Not Found`: Book with specified ID does not exist
- `503 Service Unavailable`: Database connection failed

**Example Response:**
```json
{
  "id": 1,
  "title": "The Great Gatsby",
  "author": "F. Scott Fitzgerald",
  "pages": 180,
  "language": "English",
  "genre": "Fiction",
  "coverImageUrl": "https://example.com/cover1.jpg"
}
```

---

### POST /api/books

Create a new book.

**Request**
- Method: `POST`
- Path: `/api/books`
- Content-Type: `application/json`
- Body: `CreateBookDto`

**CreateBookDto Schema:**
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| title | string | Yes | Max length 500 |
| author | string | Yes | Max length 300 |
| pages | integer | Yes | Range: 1-50000 |
| language | string | Yes | Non-empty |
| genre | string | Yes | Non-empty |
| coverImageUrl | string | No | Optional |

**Response**
- `201 Created`: Book created successfully with `Location` header pointing to `/api/books/{id}`
- `400 Bad Request`: Validation failed (missing required fields, invalid data annotations)
- `503 Service Unavailable`: Database connection failed

**Example Request:**
```json
{
  "title": "1984",
  "author": "George Orwell",
  "pages": 328,
  "language": "English",
  "genre": "Dystopian",
  "coverImageUrl": "https://example.com/1984-cover.jpg"
}
```

**Example Response:**
```json
{
  "id": 2,
  "title": "1984",
  "author": "George Orwell",
  "pages": 328,
  "language": "English",
  "genre": "Dystopian",
  "coverImageUrl": "https://example.com/1984-cover.jpg"
}
```

---

### PUT /api/books/{id}

Update an existing book.

**Request**
- Method: `PUT`
- Path: `/api/books/{id}`
- Content-Type: `application/json`
- Body: `UpdateBookDto`
- Route constraint: `id` must be a positive integer

**UpdateBookDto Schema:**
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| title | string | Yes | Max length 200 |
| author | string | Yes | Required |
| pages | integer | Yes | Range: 1-99999 |
| language | string | No | Optional |
| genre | string | No | Optional |
| coverImageUrl | string | No | Optional |

**Response**
- `200 OK`: Book updated successfully
- `400 Bad Request`: Validation failed or invalid ID
- `404 Not Found`: Book with specified ID does not exist
- `503 Service Unavailable`: Database connection failed

**Example Request:**
```json
{
  "title": "Nineteen Eighty-Four",
  "author": "George Orwell",
  "pages": 328,
  "language": "English",
  "genre": "Dystopian",
  "coverImageUrl": "https://example.com/1984-cover.jpg"
}
```

---

### DELETE /api/books/{id}

Delete a book.

**Request**
- Method: `DELETE`
- Path: `/api/books/{id}`
- Route constraint: `id` must be a positive integer

**Response**
- `204 No Content`: Book deleted successfully
- `400 Bad Request`: Invalid ID
- `404 Not Found`: Book with specified ID does not exist
- `503 Service Unavailable`: Database connection failed

---

## Validation Rules

### Book Entity
- `Id`: Auto-generated integer (not required in DTOs)
- `Title`: String, required, max 500 chars (Create), max 200 chars (Update)
- `Author`: String, required, max 300 chars (Create), required (Update)
- `Pages`: Integer, required, range 1-50000 (Create), range 1-99999 (Update)
- `Language`: String, required, non-empty
- `Genre`: String, required, non-empty
- `CoverImageUrl`: String, optional

### Error Response Format

All error responses return JSON with the following structure:

```json
{
  "error": "Descriptive error message"
}
```

### Model Validation Errors

When model validation fails (e.g., missing required fields), the API returns `400 Bad Request` with the model state errors:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": ["Title is required."],
    "Author": ["Author must not exceed 300 characters."]
  }
}
```

---

## HTTP Status Codes

| Status Code | Meaning |
|-------------|---------|
| 200 OK | Request succeeded |
| 201 Created | Resource created (includes Location header) |
| 204 No Content | Resource deleted successfully |
| 400 Bad Request | Invalid request (validation error or invalid ID) |
| 404 Not Found | Resource does not exist |
| 503 Service Unavailable | Database connection failed |
| 500 Internal Server Error | Unexpected error (handled by middleware) |

---

## Routing Conventions

- Controller uses `[ApiController]` attribute for automatic model validation
- Route template: `api/[controller]` resolves to `/api/books`
- Route constraints: `{id:int}` enforces integer IDs
- All actions return `ActionResult<T>` for flexible response types