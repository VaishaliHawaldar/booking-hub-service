using BookingHub.Service.Dtos;
using BookingHub.Service.Repositories;

namespace BookingHub.Service.Services;

public class ShowtimeService : IShowtimeService
{
    private readonly IShowtimeRepository _repository;

    public ShowtimeService(IShowtimeRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ShowtimeResponse>> GetByMovieIdAsync(string movieId, CancellationToken cancellationToken = default)
    {
        var showtimes = await _repository.GetByMovieIdAsync(movieId, cancellationToken);
        return showtimes.Select(s => s.ToResponse()).ToList();
    }
}
