using BookingHub.Service.Models;

namespace BookingHub.Service.Repositories;

/// <summary>
/// Abstraction over the movie data store. The rest of the app depends on this
/// interface, not on MongoDB directly, so the storage technology can change
/// without touching controllers or services (Dependency Inversion).
/// </summary>
public interface IMovieRepository
{
    Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Movie?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
