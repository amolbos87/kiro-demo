// Feature: books-api, Property 1: Create-then-retrieve round trip
// Feature: books-api, Property 2: Invalid input is always rejected
// Feature: books-api, Property 3: Update reflects submitted fields
// Feature: books-api, Property 4: Delete removes the record
// Feature: books-api, Property 5: Non-existent Id always returns 404
global using Xunit;

global using BooksApi.Tests.Integration;
global using BooksApi.Tests.Controllers;
