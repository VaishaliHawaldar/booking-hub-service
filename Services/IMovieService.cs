using BookingHub.Service.Models;

namespace BookingHub.Service.Services;

/// <summary>
/// Application-facing operations for movies. Controllers depend on this rather
/// than on the repository, giving us a place to put business rules (validation,
/// caching, cross-entity logic) without bloating the controller or the data layer.
/// </summary>
public interface IMovieService
{
    Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Movie?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
