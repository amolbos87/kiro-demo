using System;
using System.ComponentModel.DataAnnotations;

namespace BooksApi.DTOs;

public class UpdateBookDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author is required.")]
    public string Author { get; set; } = string.Empty;

    [Range(1, 99999, ErrorMessage = "Pages must be between 1 and 99999.")]
    public int Pages { get; set; }

    public string Language { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
}
