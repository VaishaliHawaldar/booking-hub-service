using BookingHub.Service.Models;
using BookingHub.Service.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookingHub.Service.Data;

/// <summary>
/// Seeds the movies collection with sample data on startup if it is empty.
/// Keeps local development working without a manual data-import step. Safe to
/// run repeatedly — it only inserts when the collection has no documents.
/// </summary>
public static class MovieSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var client = services.GetRequiredService<IMongoClient>();
        var settings = services.GetRequiredService<IOptions<MongoDbSettings>>().Value;

        var collection = client
            .GetDatabase(settings.DatabaseName)
            .GetCollection<Movie>(settings.MoviesCollectionName);

        if (await collection.EstimatedDocumentCountAsync(cancellationToken: cancellationToken) > 0)
        {
            return; // Already seeded.
        }

        var movies = new List<Movie>
        {
            new() { Title = "Interstellar", Genre = "Sci-Fi", DurationMinutes = 169, Language = "English",
                    Rating = 8.7, ReleaseDate = new DateOnly(2014, 11, 7),
                    PosterUrl = "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg" },
            new() { Title = "Inception", Genre = "Action", DurationMinutes = 148, Language = "English",
                    Rating = 8.8, ReleaseDate = new DateOnly(2010, 7, 16),
                    PosterUrl = "https://image.tmdb.org/t/p/w500/oYuLEt3zVCKq57qu2F8dT7NIa6f.jpg" },
            new() { Title = "Dune: Part Two", Genre = "Sci-Fi", DurationMinutes = 166, Language = "English",
                    Rating = 8.5, ReleaseDate = new DateOnly(2024, 3, 1),
                    PosterUrl = "https://image.tmdb.org/t/p/w500/1pdfLvkbY9ohJlCjQH2CZjjYVvJ.jpg" },
            new() { Title = "Spirited Away", Genre = "Animation", DurationMinutes = 125, Language = "Japanese",
                    Rating = 8.6, ReleaseDate = new DateOnly(2001, 7, 20),
                    PosterUrl = "https://image.tmdb.org/t/p/w500/39wmItIWsg5sZMyRUHLkWBcuVCM.jpg" },
            new() { Title = "The Dark Knight", Genre = "Action", DurationMinutes = 152, Language = "English",
                    Rating = 9.0, ReleaseDate = new DateOnly(2008, 7, 18),
                    PosterUrl = "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg" },
        };

        await collection.InsertManyAsync(movies, cancellationToken: cancellationToken);
    }
}
