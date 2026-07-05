using BookingHub.Service.Models;
using BookingHub.Service.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookingHub.Service.Repositories;

/// <summary>
/// MongoDB-backed implementation of <see cref="IShowtimeRepository"/>.
/// Holds a handle to the "showtimes" collection and translates domain calls into
/// driver queries.
/// </summary>
public class ShowtimeRepository : IShowtimeRepository
{
    private readonly IMongoCollection<Showtime> _showtimes;

    // IMongoClient is injected (registered as a singleton in Program.cs) because
    // the driver's client is thread-safe and manages its own connection pool —
    // creating one per request would exhaust connections.
    public ShowtimeRepository(IMongoClient client, IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var database = client.GetDatabase(settings.DatabaseName);
        _showtimes = database.GetCollection<Showtime>(settings.ShowtimesCollectionName);
    }

    public async Task<IReadOnlyList<Showtime>> GetByMovieIdAsync(string movieId, CancellationToken cancellationToken = default)
    {
        // Guard against malformed ids so a bad string returns an empty list
        // instead of throwing when the driver tries to parse it as an ObjectId.
        if (!ObjectId.TryParse(movieId, out _))
        {
            return Array.Empty<Showtime>();
        }

        return await _showtimes
            .Find(s => s.MovieId == movieId)
            .ToListAsync(cancellationToken);
    }
}
