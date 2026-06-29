using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
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
        // Authority points at the Auth0 tenant; the middleware fetches the
        // OIDC discovery document and JWKS (signing keys) from here.
        options.Authority = auth0Domain;
        options.Audience = auth0Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = NormalizeIssuer(auth0Domain),
            ValidateAudience = true,
            ValidAudience = auth0Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Auth0 puts the subject in the standard "sub" claim.
            NameClaimType = "sub",
        };
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // interactive API docs at /scalar/v1
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "BookingHub")); // Swagger UI at /swagger
}

app.UseHttpsRedirection();

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
