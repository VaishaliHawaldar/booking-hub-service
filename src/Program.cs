using BookingHub.Service.Middleware;
using BookingHub.Service.Repositories;
using BookingHub.Service.Services;
using BookingHub.Service.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// ---------------------------------------------------------------------------
// MongoDB wiring (Options pattern + layered data access).
//
//   Controller  ->  IMovieService  ->  IMovieRepository  ->  MongoDB driver
//
// Each layer depends on an abstraction, so any one can be swapped or tested in
// isolation. Lifetimes below are deliberate (see comments).
// ---------------------------------------------------------------------------

// 1. Bind the "MongoDb" config section to a strongly-typed options object.
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));

// 2. Register the Mongo client as a SINGLETON. The driver's IMongoClient is
//    thread-safe and owns a connection pool, so the whole app shares one.
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    if (string.IsNullOrWhiteSpace(settings.ConnectionString))
    {
        throw new InvalidOperationException(
            "MongoDb:ConnectionString is not configured. Set it via user-secrets " +
            "(dotnet user-secrets set \"MongoDb:ConnectionString\" \"<uri>\"), the " +
            "MongoDb__ConnectionString environment variable, or appsettings.Development.json. " +
            "For a local server: docker run -d -p 27017:27017 --name mongo mongo:7");
    }
    return new MongoClient(settings.ConnectionString);
});

// 3. Register the data + service layers as SCOPED (one instance per request).
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
builder.Services.AddScoped<IShowtimeService, ShowtimeService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    // Advertise a Bearer (JWT) security scheme in the OpenAPI document so the
    // Swagger UI renders an "Authorize" button for pasting an access token.
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Paste your Auth0 access token. The 'Bearer ' prefix is added automatically.",
        };

        // Apply the scheme globally so every operation shows the lock icon.
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            }] = Array.Empty<string>(),
        });

        return Task.CompletedTask;
    });
});

// ---------------------------------------------------------------------------
// Auth0 (Okta) JWT validation.
//
// The Next.js frontend signs users in with Auth0 and forwards the Auth0
// access token to this service as a Bearer token. We validate that token
// here against the Auth0 tenant's OIDC metadata (signing keys, issuer, etc.).
//
// Configure the tenant in appsettings*.json (or env vars / user-secrets):
//   "Auth0": {
//     "Domain":   "https://your-tenant.us.auth0.com",  // the issuer URL
//     "Audience": "https://api.bookinghub"              // your API identifier
//   }
// ---------------------------------------------------------------------------
var auth0Domain = builder.Configuration["Auth0:Domain"];
var auth0Audience = builder.Configuration["Auth0:Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = NormalizeIssuer(auth0Domain);
        options.Audience = auth0Audience;
    });

builder.Services.AddAuthorization();

// Allow the Next.js frontend to call this API from the browser.
const string FrontendCors = "frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:3000"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Every showtimes query filters by movieId, so back it with an index. Runs in all
// environments (index creation is idempotent — an identical definition is a no-op).
await BookingHub.Service.Data.ShowtimeSeeder.EnsureIndexesAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // interactive API docs at /scalar/v1
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "BookingHub")); // Swagger UI at /swagger

    // Populate the movies collection with sample data on first run, then attach
    // sample showtimes to those movies.
    await BookingHub.Service.Data.MovieSeeder.SeedAsync(app.Services);
    await BookingHub.Service.Data.ShowtimeSeeder.SeedAsync(app.Services);
}

app.UseHttpsRedirection();

app.UseRequestDuration();

app.UseCors(FrontendCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Auth0 issuer URLs always end with a trailing slash in the "iss" claim.
static string? NormalizeIssuer(string? domain)
{
    if (string.IsNullOrWhiteSpace(domain)) return domain;
    return domain.EndsWith('/') ? domain : domain + "/";
}
