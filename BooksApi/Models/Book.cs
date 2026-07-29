using System;
using System.ComponentModel.DataAnnotations;

namespace BooksApi.Models;

public class Book
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Author { get; set; } = string.Empty;

    [Range(1, 50000)]
    public int Pages { get; set; }

    [Required]
    public string Language { get; set; } = string.Empty;

    [Required]
    public string Genre { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }
}
