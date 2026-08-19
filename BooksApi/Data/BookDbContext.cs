using System;
using BooksApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasAnnotation("sqlite:Autoincrement", true);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Author)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(e => e.Pages)
                .IsRequired();

            entity.Property(e => e.Language)
                .IsRequired();

            entity.Property(e => e.Genre)
                .IsRequired();

            entity.Property(e => e.CoverImageUrl)
                .IsRequired(false);
        });
    }
}
