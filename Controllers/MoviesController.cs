using BookingHub.Service.Models;
using BookingHub.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires a valid Auth0 (Okta) access token. See Program.cs for validation setup.
public class MoviesController(IMovieService movieService) : ControllerBase
{
    private readonly IMovieService _movieService = movieService;

    /// <summary>Returns the list of bookable movies.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Movie>>> GetAll(CancellationToken cancellationToken)
    {
        var movies = await _movieService.GetAllAsync(cancellationToken);
        return Ok(movies);
    }

    /// <summary>Returns a single movie by its MongoDB id, or 404 if not found.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Movie>> GetById(string id, CancellationToken cancellationToken)
    {
        var movie = await _movieService.GetByIdAsync(id, cancellationToken);
        return movie is null ? NotFound() : Ok(movie);
    }
}
