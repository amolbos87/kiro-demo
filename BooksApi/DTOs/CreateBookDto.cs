using System;
using System.ComponentModel.DataAnnotations;

namespace BooksApi.DTOs;

public class CreateBookDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(500, ErrorMessage = "Title must not exceed 500 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author is required.")]
    [MaxLength(300, ErrorMessage = "Author must not exceed 300 characters.")]
    public string Author { get; set; } = string.Empty;

    [Range(1, 50000, ErrorMessage = "Pages must be between 1 and 50000.")]
    public int Pages { get; set; }

    [Required(ErrorMessage = "Language is required.")]
    public string Language { get; set; } = string.Empty;

    [Required(ErrorMessage = "Genre is required.")]
    public string Genre { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }
}
