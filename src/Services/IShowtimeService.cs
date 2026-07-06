using BookingHub.Service.Dtos;

namespace BookingHub.Service.Services;

public interface IShowtimeService
{
    Task<IReadOnlyList<ShowtimeResponse>> GetByMovieIdAsync(string movieId, CancellationToken cancellationToken = default);
}
