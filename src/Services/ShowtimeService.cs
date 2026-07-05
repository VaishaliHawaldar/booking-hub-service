using BookingHub.Service.Models;
using BookingHub.Service.Repositories;

namespace BookingHub.Service.Services;

public class ShowtimeService : IShowtimeService
{
    private readonly IShowtimeRepository _repository;

    public ShowtimeService(IShowtimeRepository repository) => _repository = repository;

    public Task<IReadOnlyList<Showtime>> GetByMovieIdAsync(string movieId, CancellationToken cancellationToken = default) =>
        _repository.GetByMovieIdAsync(movieId, cancellationToken);
}
