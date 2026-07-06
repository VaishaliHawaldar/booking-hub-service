using BookingHub.Service.Models;

namespace BookingHub.Service.Dtos;

/// <summary>Maps the <see cref="Showtime"/> persistence entity to its HTTP contract.</summary>
public static class ShowtimeMapping
{
    public static ShowtimeResponse ToResponse(this Showtime s) =>
        new(s.Id!, s.MovieId!, s.Screen, s.StartTime, s.BasePrice, s.TotalSeats);
}
