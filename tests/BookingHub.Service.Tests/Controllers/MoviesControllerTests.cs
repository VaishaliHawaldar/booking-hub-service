using BookingHub.Service.Controllers;
using BookingHub.Service.Models;
using BookingHub.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BookingHub.Service.Tests.Controllers;

public class MoviesControllerTests
{
    private readonly Mock<IMovieService> _service = new(MockBehavior.Strict);
    private readonly MoviesController _controller;

    public MoviesControllerTests() => _controller = new MoviesController(_service.Object);

    [Fact]
    public async Task GetAll_Returns200_WithMoviesFromService()
    {
        var movies = new List<Movie> { new() { Id = "1", Title = "Interstellar" } };
        _service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(movies);

        var actionResult = await _controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(movies, ok.Value);
    }

    [Fact]
    public async Task GetById_Returns200_WhenMovieExists()
    {
        var movie = new Movie { Id = "abc", Title = "Inception" };
        _service.Setup(s => s.GetByIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync(movie);

        var actionResult = await _controller.GetById("abc", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(movie, ok.Value);
    }

    [Fact]
    public async Task GetById_Returns404_WhenMovieMissing()
    {
        _service
            .Setup(s => s.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var actionResult = await _controller.GetById("missing", CancellationToken.None);

        Assert.IsType<NotFoundResult>(actionResult.Result);
    }
}
