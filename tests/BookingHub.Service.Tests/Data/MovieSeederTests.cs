using BookingHub.Service.Data;
using BookingHub.Service.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Service.Tests.Data;

[Collection("Mongo")]
public class MovieSeederTests(MongoTestFixture fixture)
{
    // The seeder resolves IMongoClient + IOptions<MongoDbSettings> from a
    // provider, so build a minimal one pointing at the test database.
    private IServiceProvider BuildServices(MongoTestContext ctx)
    {
        var services = new ServiceCollection();
        services.AddSingleton(ctx.Client);
        services.AddSingleton(ctx.Options);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task SeedAsync_InsertsSampleMovies_WhenCollectionIsEmpty()
    {
        var ctx = new MongoTestContext(fixture);

        await MovieSeeder.SeedAsync(BuildServices(ctx));

        var count = await ctx.MoviesCollection.CountDocumentsAsync(FilterDefinition<Models.Movie>.Empty);
        Assert.Equal(5, count);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_WhenCalledTwice()
    {
        var ctx = new MongoTestContext(fixture);
        var services = BuildServices(ctx);

        await MovieSeeder.SeedAsync(services);
        await MovieSeeder.SeedAsync(services); // second call should be a no-op

        var count = await ctx.MoviesCollection.CountDocumentsAsync(FilterDefinition<Models.Movie>.Empty);
        Assert.Equal(5, count);
    }

    [Fact]
    public async Task SeedAsync_DoesNotInsert_WhenCollectionAlreadyHasData()
    {
        var ctx = new MongoTestContext(fixture);
        await ctx.SeedMoviesAsync(MongoTestContext.SampleMovie("Pre-existing"));

        await MovieSeeder.SeedAsync(BuildServices(ctx));

        var count = await ctx.MoviesCollection.CountDocumentsAsync(FilterDefinition<Models.Movie>.Empty);
        Assert.Equal(1, count); // untouched — still just the one we inserted
    }
}
