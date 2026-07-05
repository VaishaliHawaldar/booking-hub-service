using BookingHub.Service.Models;
using BookingHub.Service.Repositories;
using BookingHub.Service.Services;
using Moq;

namespace BookingHub.Service.Tests.Services;

public class MovieServiceTests
{
    private readonly Mock<IMovieRepository> _repository = new(MockBehavior.Strict);
    private readonly MovieService _service;

    public MovieServiceTests() => _service = new MovieService(_repository.Object);

    [Fact]
    public async Task GetAllAsync_ReturnsWhateverTheRepositoryReturns()
    {
        var movies = new List<Movie> { new() { Id = "1", Title = "Interstellar" } };
        using var cts = new CancellationTokenSource();
        _repository.Setup(r => r.GetAllAsync(cts.Token)).ReturnsAsync(movies);

        var result = await _service.GetAllAsync(cts.Token);

        Assert.Same(movies, result);
        _repository.Verify(r => r.GetAllAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ForwardsIdAndToken_AndReturnsMovie()
    {
        var movie = new Movie { Id = "abc", Title = "Inception" };
        using var cts = new CancellationTokenSource();
        _repository.Setup(r => r.GetByIdAsync("abc", cts.Token)).ReturnsAsync(movie);

        var result = await _service.GetByIdAsync("abc", cts.Token);

        Assert.Same(movie, result);
        _repository.Verify(r => r.GetByIdAsync("abc", cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenRepositoryFindsNothing()
    {
        _repository
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var result = await _service.GetByIdAsync("missing");

        Assert.Null(result);
    }
}
