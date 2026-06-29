using BookingHub.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires a valid Auth0 (Okta) access token. See Program.cs for validation setup.
public class MoviesController : ControllerBase
{
    // Mock data — stands in for a real data store until one is wired up.
    private static readonly Movie[] Movies =
    [
        new(1, "Interstellar", "Sci-Fi", 169, "English",
            8.7, new DateOnly(2014, 11, 7),
            "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg"),
        new(2, "Inception", "Action", 148, "English",
            8.8, new DateOnly(2010, 7, 16),
            "https://image.tmdb.org/t/p/w500/oYuLEt3zVCKq57qu2F8dT7NIa6f.jpg"),
        new(3, "Dune: Part Two", "Sci-Fi", 166, "English",
            8.5, new DateOnly(2024, 3, 1),
            "https://image.tmdb.org/t/p/w500/1pdfLvkbY9ohJlCjQH2CZjjYVvJ.jpg"),
        new(4, "Spirited Away", "Animation", 125, "Japanese",
            8.6, new DateOnly(2001, 7, 20),
            "https://image.tmdb.org/t/p/w500/39wmItIWsg5sZMyRUHLkWBcuVCM.jpg"),
        new(5, "The Dark Knight", "Action", 152, "English",
            9.0, new DateOnly(2008, 7, 18),
            "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg"),
    ];

    /// <summary>Returns the list of bookable movies.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<Movie>> GetAll() => Ok(Movies);

    /// <summary>Returns a single movie by id, or 404 if not found.</summary>
    [HttpGet("{id:int}")]
    public ActionResult<Movie> GetById(int id)
    {
        var movie = Movies.FirstOrDefault(m => m.Id == id);
        return movie is null ? NotFound() : Ok(movie);
    }
}
