using BookingHub.Service.Models;
using BookingHub.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires a valid Auth0 (Okta) access token. See Program.cs for validation setup.
public class ShowtimesController(IShowtimeService showtimeService) : ControllerBase
{
    private readonly IShowtimeService _showtimeService = showtimeService;

    /// <summary>Returns the showtimes for a given movie (empty list if none or if the id is malformed).</summary>
    [HttpGet("{movieId}")]
    public async Task<ActionResult<IReadOnlyList<Showtime>>> GetByMovieId(string movieId, CancellationToken cancellationToken)
    {
        var showtimes = await _showtimeService.GetByMovieIdAsync(movieId, cancellationToken);
        return Ok(showtimes);
    }
}
