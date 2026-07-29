using System;
using System.Threading.Tasks;
using BooksApi.Data;
using BooksApi.DTOs;
using BooksApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _repository;

    public BooksController(IBookRepository repository)
    {
        _repository = repository;
    }

    // GET: api/books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetAll()
    {
        try
        {
            var books = await _repository.GetAllAsync();
            return Ok(books);
        }
        catch (DbException ex)
        {
            return StatusCode(503, new { error = "Service temporarily unavailable. Database connection failed." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    // GET: api/books/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Book>> GetById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Id must be a positive integer." });
            }

            var book = await _repository.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound(new { error = $"Book with Id {id} was not found." });
            }

            return Ok(book);
        }
        catch (DbException ex)
        {
            return StatusCode(503, new { error = "Service temporarily unavailable. Database connection failed." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    // POST: api/books
    [HttpPost]
    public async Task<ActionResult<Book>> Create(CreateBookDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Pages = dto.Pages,
                Language = dto.Language,
                Genre = dto.Genre,
                CoverImageUrl = dto.CoverImageUrl
            };

            var createdBook = await _repository.AddAsync(book);

            return CreatedAtAction(nameof(GetById), new { id = createdBook.Id }, createdBook);
        }
        catch (DbException ex)
        {
            return StatusCode(503, new { error = "Service temporarily unavailable. Database connection failed." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    // PUT: api/books/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Book>> Update(int id, UpdateBookDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Id must be a positive integer." });
            }

            var book = new Book
            {
                Id = id,
                Title = dto.Title,
                Author = dto.Author,
                Pages = dto.Pages,
                Language = dto.Language,
                Genre = dto.Genre,
                CoverImageUrl = dto.CoverImageUrl
            };

            var updatedBook = await _repository.UpdateAsync(id, book);
            if (updatedBook == null)
            {
                return NotFound(new { error = $"Book with Id {id} was not found." });
            }

            return Ok(updatedBook);
        }
        catch (DbException ex)
        {
            return StatusCode(503, new { error = "Service temporarily unavailable. Database connection failed." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }

    // DELETE: api/books/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Id must be a positive integer." });
            }

            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { error = $"Book with Id {id} was not found." });
            }

            return NoContent();
        }
        catch (DbException ex)
        {
            return StatusCode(503, new { error = "Service temporarily unavailable. Database connection failed." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }
}
