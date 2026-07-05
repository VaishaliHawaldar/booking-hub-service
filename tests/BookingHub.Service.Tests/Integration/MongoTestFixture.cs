using EphemeralMongo;

namespace BookingHub.Service.Tests.Integration;

/// <summary>
/// Spins up a real, throwaway MongoDB instance (via EphemeralMongo — a self-
/// contained mongod, no Docker required) once for the whole test run. Data-layer
/// tests run against this real server so we exercise the actual driver behaviour
/// (BSON mapping, ObjectId handling, queries) instead of a mock that could lie.
/// </summary>
public sealed class MongoTestFixture : IDisposable
{
    public IMongoRunner Runner { get; }

    public MongoTestFixture()
    {
        Runner = MongoRunner.Run(new MongoRunnerOptions
        {
            // Faster startup/teardown for tests; durability doesn't matter here.
            StandardOutputLogger = _ => { },
            StandardErrorLogger = _ => { },
        });
    }

    public string ConnectionString => Runner.ConnectionString;

    public void Dispose() => Runner.Dispose();
}

/// <summary>
/// Shares a single <see cref="MongoTestFixture"/> across every test class marked
/// with [Collection("Mongo")], so we pay the mongod startup cost only once.
/// </summary>
[CollectionDefinition("Mongo")]
public class MongoCollection : ICollectionFixture<MongoTestFixture>;
