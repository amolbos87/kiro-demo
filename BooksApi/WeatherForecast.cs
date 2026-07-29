// Feature: books-api, Property 1: Create-then-retrieve round trip
// Feature: books-api, Property 2: Invalid input is always rejected
// Feature: books-api, Property 3: Update reflects submitted fields
// Feature: books-api, Property 4: Delete removes the record
// Feature: books-api, Property 5: Non-existent Id always returns 404
namespace BooksApi;

public class WeatherForecast
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
