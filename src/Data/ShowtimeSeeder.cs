using BookingHub.Service.Models;
using BookingHub.Service.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookingHub.Service.Data;

/// <summary>
/// Bootstraps the showtimes collection: ensures the query index exists (always)
/// and seeds sample showtimes on startup if the collection is empty (Development).
/// </summary>
public static class ShowtimeSeeder
{
    /// <summary>
    /// Ensures a single-field ascending index on <c>movieId</c>. Every read filters
    /// by movie, so this backs the only query pattern. Safe to call on every startup —
    /// Mongo treats an identical index definition as a no-op.
    /// </summary>
    public static async Task EnsureIndexesAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection(services);

        var indexKeys = Builders<Showtime>.IndexKeys.Ascending(s => s.MovieId);
        var indexModel = new CreateIndexModel<Showtime>(indexKeys);

        await collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Inserts sample showtimes if the collection is empty. Attaches real movie ids
    /// from the seeded movies collection, so <c>GET /api/showtimes/{movieId}</c>
    /// returns data locally. No-op if there are no movies to reference yet.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var client = services.GetRequiredService<IMongoClient>();
        var settings = services.GetRequiredService<IOptions<MongoDbSettings>>().Value;
        var database = client.GetDatabase(settings.DatabaseName);

        var collection = database.GetCollection<Showtime>(settings.ShowtimesCollectionName);

        if (await collection.EstimatedDocumentCountAsync(cancellationToken: cancellationToken) > 0)
        {
            return; // Already seeded.
        }

        // Showtimes reference movies, so pull a couple of seeded movie ids first.
        var movies = await database
            .GetCollection<Movie>(settings.MoviesCollectionName)
            .Find(FilterDefinition<Movie>.Empty)
            .Limit(2)
            .ToListAsync(cancellationToken);

        if (movies.Count == 0)
        {
            return; // No movies to attach showtimes to yet.
        }

        var today = DateTime.UtcNow.Date;
        var showtimes = new List<Showtime>();

        foreach (var movie in movies)
        {
            showtimes.Add(new Showtime
            {
                MovieId = movie.Id,
                Screen = "Screen 1",
                StartTime = today.AddHours(14),
                BasePrice = 12.50m,
                TotalSeats = 120,
            });
            showtimes.Add(new Showtime
            {
                MovieId = movie.Id,
                Screen = "Screen 2",
                StartTime = today.AddHours(19),
                BasePrice = 15.00m,
                TotalSeats = 80,
            });
        }

        await collection.InsertManyAsync(showtimes, cancellationToken: cancellationToken);
    }

    private static IMongoCollection<Showtime> GetCollection(IServiceProvider services)
    {
        var client = services.GetRequiredService<IMongoClient>();
        var settings = services.GetRequiredService<IOptions<MongoDbSettings>>().Value;

        return client
            .GetDatabase(settings.DatabaseName)
            .GetCollection<Showtime>(settings.ShowtimesCollectionName);
    }
}
