using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingHub.Service.Models;

/// <summary>
/// A scheduled screening of a <see cref="Movie"/> on a particular screen.
/// Belongs to one movie via <see cref="MovieId"/>.
/// </summary>
public class Showtime
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>The owning movie's ObjectId (24-char hex string).</summary>
    [BsonElement("movieId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? MovieId { get; set; }

    [BsonElement("screen")]
    public string Screen { get; set; } = string.Empty;

    [BsonElement("startTime")]
    public DateTime StartTime { get; set; }

    [BsonElement("basePrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal BasePrice { get; set; }

    [BsonElement("totalSeats")]
    public int TotalSeats { get; set; }
}
