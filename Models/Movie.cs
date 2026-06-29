namespace BookingHub.Service.Models;

/// <summary>
/// A movie available for booking. This is the shape returned by the API.
/// </summary>
public record Movie(
    int Id,
    string Title,
    string Genre,
    int DurationMinutes,
    string Language,
    double Rating,
    DateOnly ReleaseDate,
    string PosterUrl
);
