using BookingHub.Service.Models;
using BookingHub.Service.Repositories;

namespace BookingHub.Service.Services;

/// <summary>
/// Default <see cref="IMovieService"/> implementation. Today it simply delegates
/// to the repository; it exists so future business rules have a home that isn't
/// the controller (HTTP concerns) or the repository (persistence concerns).
/// </summary>
public class MovieService : IMovieService
{
    private readonly IMovieRepository _repository;

    public MovieService(IMovieRepository repository) => _repository = repository;

    public Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(cancellationToken);

    public Task<Movie?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, cancellationToken);
}
