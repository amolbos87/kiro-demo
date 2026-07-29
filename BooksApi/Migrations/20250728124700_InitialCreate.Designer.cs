using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BooksApi.Migrations;

[DbContext(typeof(BookDbContext))]
partial class BookDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        builder.HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Sqlite:Autoincrement", true);

        modelBuilder.Entity("BooksApi.Models.Book", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("INTEGER")
                .HasAnnotation("Sqlite:Autoincrement", true);

            b.Property<string>("Author")
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnType("TEXT");

            b.Property<string>("CoverImageUrl")
                .HasColumnType("TEXT");

            b.Property<string>("Genre")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("Language")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<int>("Pages")
                .HasColumnType("INTEGER");

            b.Property<string>("Title")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("TEXT");

            b.HasKey("Id");

            b.ToTable("Books");
        });

        OnBuildModel(modelBuilder);
    }

    partial void OnBuildModel(ModelBuilder modelBuilder);
}
