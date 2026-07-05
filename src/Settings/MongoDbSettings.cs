namespace BookingHub.Service.Settings;

/// <summary>
/// Strongly-typed MongoDB configuration. Bound from the "MongoDb" section of
/// appsettings*.json via the Options pattern so connection details stay out of
/// code and can be overridden per environment (env vars / user-secrets).
/// </summary>
public class MongoDbSettings
{
    /// <summary>Connection string, e.g. "mongodb://localhost:27017".</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Name of the database that holds the collections.</summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>Name of the collection that stores <see cref="Models.Movie"/> documents.</summary>
    public string MoviesCollectionName { get; set; } = "movies";

    /// <summary>Name of the collection that stores <see cref="Models.Showtime"/> documents.</summary>
    public string ShowtimesCollectionName { get; set; } = "showtimes";
}
