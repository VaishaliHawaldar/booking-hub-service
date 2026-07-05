using BookingHub.Service.Models;
using BookingHub.Service.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookingHub.Service.Tests.Integration;

/// <summary>
/// Per-test helper that points at the shared Mongo server but uses a unique
/// database name, so tests are isolated from each other and can run in any order.
/// </summary>
public sealed class MongoTestContext
{
    public IMongoClient Client { get; }
    public IOptions<MongoDbSettings> Options { get; }
    public MongoDbSettings Settings { get; }

    public MongoTestContext(MongoTestFixture fixture)
    {
        Client = new MongoClient(fixture.ConnectionString);
        Settings = new MongoDbSettings
        {
            ConnectionString = fixture.ConnectionString,
            // Unique DB per context → full isolation between tests.
            DatabaseName = $"test_{Guid.NewGuid():N}",
            MoviesCollectionName = "movies",
        };
        Options = Microsoft.Extensions.Options.Options.Create(Settings);
    }

    public IMongoCollection<Movie> MoviesCollection =>
        Client.GetDatabase(Settings.DatabaseName)
              .GetCollection<Movie>(Settings.MoviesCollectionName);

    /// <summary>Inserts the given movies and returns them with their assigned ids.</summary>
    public async Task<IReadOnlyList<Movie>> SeedMoviesAsync(params Movie[] movies)
    {
        await MoviesCollection.InsertManyAsync(movies);
        return movies;
    }

    public static Movie SampleMovie(string title = "Interstellar") => new()
    {
        Title = title,
        Genre = "Sci-Fi",
        DurationMinutes = 169,
        Language = "English",
        Rating = 8.7,
        ReleaseDate = new DateOnly(2014, 11, 7),
        PosterUrl = "https://example.com/poster.jpg",
    };
}
