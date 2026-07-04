# BookingHub.Service

ASP.NET Core 9 Web API (C#) — the backend for a movie-booking app. A Next.js
frontend signs users in with Auth0 and calls this API with a Bearer token.

## Tech stack

- **.NET 9** / ASP.NET Core Web API, `Nullable` + `ImplicitUsings` enabled
- **MongoDB** via `MongoDB.Driver` 3.x for persistence
- **Auth0 (OIDC/JWT)** for authentication — validated in `Program.cs`
- **OpenAPI** docs: Scalar at `/scalar/v1`, Swagger UI at `/swagger` (Development only)

## Architecture — layered, dependency-inverted

Request flow:

```
HTTP → Controller → IMovieService → IMovieRepository → MongoDB.Driver → MongoDB
```

Every layer depends on an **interface**, wired through the built-in DI container.
When adding a feature, follow the same seams — don't let MongoDB types or business
rules leak into controllers.

| Layer        | Folder           | Responsibility                                  |
|--------------|------------------|-------------------------------------------------|
| Controller   | `Controllers/`   | HTTP only: map requests ↔ service, return status |
| Service      | `Services/`      | Business rules; controllers depend on `I*Service` |
| Repository   | `Repositories/`  | All data access; hides the Mongo driver          |
| Model        | `Models/`        | Persistence documents (BSON attributes)          |
| Settings     | `Settings/`      | Strongly-typed config (Options pattern)          |
| Data         | `Data/`          | Seeders / one-off data helpers                   |

### DI lifetimes (important)
- `IMongoClient` → **Singleton** (thread-safe, owns the connection pool — one per app)
- Repositories & Services → **Scoped** (one per request)

### Conventions
- Data-access methods are **async** and take a `CancellationToken` threaded from the controller.
- Entity ids are MongoDB **`ObjectId` strings** (24-char hex), exposed as `string Id`.
  Repositories validate the id format and return `null`/404 on malformed input rather than throwing.
- Config lives in `appsettings*.json` sections bound via `builder.Services.Configure<T>(...)`.
  Never hardcode connection strings/secrets — override per env with env vars or user-secrets.

## Commands

```bash
dotnet build                 # compile (keep it 0 warnings)
dotnet run                   # run on http://localhost:5002 (Development)
dotnet run --launch-profile https   # also https://localhost:7049
```

Requires a local MongoDB. Quick start:
```bash
docker run -d -p 27017:27017 --name mongo mongo:7
```

## Configuration (appsettings.json)

- `MongoDb` — `ConnectionString`, `DatabaseName` (`bookinghub`), `MoviesCollectionName` (`movies`)
- `Auth0` — `Domain` (issuer URL), `Audience` (API identifier)
- `Cors:AllowedOrigins` — defaults to `http://localhost:3000` (the frontend)

## Auth

All endpoints require a valid Auth0 access token (`[Authorize]`). For manual testing,
paste a real token into `BookingHub.Service.http` (`@accessToken`) or the Swagger
"Authorize" button. Tokens are validated against the Auth0 tenant's OIDC metadata.

## Data seeding

In **Development**, `Data/MovieSeeder` inserts 5 sample movies on startup if the
`movies` collection is empty (idempotent). No manual import needed for local dev.

## API

- `GET /api/movies` — all movies
- `GET /api/movies/{id}` — one movie by ObjectId string; 404 if not found
