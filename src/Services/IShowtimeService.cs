using BookingHub.Service.Models;

namespace BookingHub.Service.Services;

public interface IShowtimeService
{
    Task<IReadOnlyList<Showtime>> GetByMovieIdAsync(string movieId, CancellationToken cancellationToken = default);
}
