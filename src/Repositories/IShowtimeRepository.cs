using BookingHub.Service.Models;

namespace BookingHub.Service.Repositories;

public interface IShowtimeRepository
{
    Task<IReadOnlyList<Showtime>> GetByMovieIdAsync(string movieId, CancellationToken cancellationToken = default);
}
