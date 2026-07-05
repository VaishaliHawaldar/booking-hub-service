using BookingHub.Service.Models;
using BookingHub.Service.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookingHub.Service.Repositories;

/// <summary>
/// MongoDB-backed implementation of <see cref="IMovieRepository"/>.
/// Holds a handle to the "movies" collection and translates domain calls into
/// driver queries.
/// </summary>
public class MovieRepository : IMovieRepository
{
    private readonly IMongoCollection<Movie> _movies;

    // IMongoClient is injected (registered as a singleton in Program.cs) because
    // the driver's client is thread-safe and manages its own connection pool —
    // creating one per request would exhaust connections.
    public MovieRepository(IMongoClient client, IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var database = client.GetDatabase(settings.DatabaseName);
        _movies = database.GetCollection<Movie>(settings.MoviesCollectionName);
    }

    public async Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _movies
            .Find(FilterDefinition<Movie>.Empty)
            .ToListAsync(cancellationToken);
    }

    public async Task<Movie?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        // Guard against malformed ids so a bad string returns "not found"
        // instead of throwing when the driver tries to parse it as an ObjectId.
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        return await _movies
            .Find(m => m.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
