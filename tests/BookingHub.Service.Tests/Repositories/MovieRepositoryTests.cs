using BookingHub.Service.Repositories;
using BookingHub.Service.Tests.Integration;
using MongoDB.Bson;

namespace BookingHub.Service.Tests.Repositories;

[Collection("Mongo")]
public class MovieRepositoryTests(MongoTestFixture fixture)
{
    private MovieRepository CreateRepository(MongoTestContext ctx) => new(ctx.Client, ctx.Options);

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenCollectionIsEmpty()
    {
        var ctx = new MongoTestContext(fixture);
        var repo = CreateRepository(ctx);

        var result = await repo.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllStoredMovies()
    {
        var ctx = new MongoTestContext(fixture);
        await ctx.SeedMoviesAsync(
            MongoTestContext.SampleMovie("Interstellar"),
            MongoTestContext.SampleMovie("Inception"));
        var repo = CreateRepository(ctx);

        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.Title == "Interstellar");
        Assert.Contains(result, m => m.Title == "Inception");
    }

    [Fact]
    public async Task GetAllAsync_RoundTripsAllFields()
    {
        var ctx = new MongoTestContext(fixture);
        var seeded = (await ctx.SeedMoviesAsync(MongoTestContext.SampleMovie()))[0];
        var repo = CreateRepository(ctx);

        var movie = Assert.Single(await repo.GetAllAsync());

        // The Mongo driver assigns an ObjectId on insert.
        Assert.False(string.IsNullOrEmpty(movie.Id));
        Assert.Equal(seeded.Title, movie.Title);
        Assert.Equal(seeded.Genre, movie.Genre);
        Assert.Equal(seeded.DurationMinutes, movie.DurationMinutes);
        Assert.Equal(seeded.Language, movie.Language);
        Assert.Equal(seeded.Rating, movie.Rating);
        Assert.Equal(seeded.ReleaseDate, movie.ReleaseDate);
        Assert.Equal(seeded.PosterUrl, movie.PosterUrl);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMovie_WhenIdExists()
    {
        var ctx = new MongoTestContext(fixture);
        await ctx.SeedMoviesAsync(MongoTestContext.SampleMovie("Dune"));
        var repo = CreateRepository(ctx);
        var existingId = (await repo.GetAllAsync())[0].Id!;

        var result = await repo.GetByIdAsync(existingId);

        Assert.NotNull(result);
        Assert.Equal(existingId, result!.Id);
        Assert.Equal("Dune", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenIdIsWellFormedButMissing()
    {
        var ctx = new MongoTestContext(fixture);
        var repo = CreateRepository(ctx);

        var result = await repo.GetByIdAsync(ObjectId.GenerateNewId().ToString());

        Assert.Null(result);
    }

    [Theory]
    [InlineData("not-an-object-id")]
    [InlineData("123")]
    [InlineData("")]
    public async Task GetByIdAsync_ReturnsNull_WhenIdIsMalformed(string malformedId)
    {
        var ctx = new MongoTestContext(fixture);
        // Seed data to prove it's the guard (not an empty collection) returning null.
        await ctx.SeedMoviesAsync(MongoTestContext.SampleMovie());
        var repo = CreateRepository(ctx);

        var result = await repo.GetByIdAsync(malformedId);

        Assert.Null(result);
    }
}
