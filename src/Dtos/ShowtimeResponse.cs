namespace BookingHub.Service.Dtos;

/// <summary>
/// The HTTP-facing shape of a showtime, decoupled from the MongoDB persistence
/// entity so that BSON/storage concerns never leak across the API boundary.
/// </summary>
public record ShowtimeResponse(
    string Id,
    string MovieId,
    string Screen,
    DateTime StartTime,
    decimal BasePrice,
    int TotalSeats);
