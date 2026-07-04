using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingHub.Service.Models;

/// <summary>
/// A movie available for booking. This is both the persistence document
/// (stored in MongoDB) and the shape returned by the API.
/// </summary>
public class Movie
{
    /// <summary>
    /// MongoDB's primary key. Stored as an ObjectId in the database but
    /// surfaced as a string in C#/JSON so callers never deal with the
    /// BSON type directly. Assigned by MongoDB on insert.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("genre")]
    public string Genre { get; set; } = string.Empty;

    [BsonElement("durationMinutes")]
    public int DurationMinutes { get; set; }

    [BsonElement("language")]
    public string Language { get; set; } = string.Empty;

    [BsonElement("rating")]
    public double Rating { get; set; }

    [BsonElement("releaseDate")]
    public DateOnly ReleaseDate { get; set; }

    [BsonElement("posterUrl")]
    public string PosterUrl { get; set; } = string.Empty;
}
