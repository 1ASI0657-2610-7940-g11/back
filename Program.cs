using FuelTrack.Api.Features.Auth.Data;
using FuelTrack.Api.Features.Auth.Domain;
using FuelTrack.Api.Features.Client.Data;
using FuelTrack.Api.Features.Client.Domain;
using FuelTrack.Api.Features.Company.Data;
using FuelTrack.Api.Features.Company.Domain;
using FuelTrack.Api.Features.Home.Data;
using FuelTrack.Api.Features.Home.Domain;
using FuelTrack.Api.Features.Orders.Data;
using FuelTrack.Api.Features.Orders.Domain;
using FuelTrack.Api.Features.Payments.Data;
using FuelTrack.Api.Features.Payments.Domain;
using FuelTrack.Api.Features.Profile.Data;
using FuelTrack.Api.Features.Profile.Domain;
using FuelTrack.Api.Features.Provider.Data;
using FuelTrack.Api.Features.Provider.Domain;
using FuelTrack.Api.Infrastructure.Auth;
using FuelTrack.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var railwayPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var connectionString = DatabaseConfiguration.BuildConnectionString(
    builder.Configuration,
    builder.Environment);
builder.Services.AddDbContext<FuelTrackDbContext>(options =>
    options.UseMySQL(connectionString));

var jwtOptions = JwtOptions.FromConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<PasswordHashService>();
builder.Services.AddScoped<TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var configuredOrigins = builder.Configuration["ALLOWED_ORIGINS"]?
            .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();
        var allowedOrigins = configuredOrigins.Length > 0
            ? configuredOrigins
            : builder.Environment.IsDevelopment()
                ? new[] { "http://localhost:5173", "http://127.0.0.1:5173" }
                : Array.Empty<string>();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddHealthChecks().AddDbContextCheck<FuelTrackDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName?.Replace('.', '_'));
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<IOrdersRepository, MySqlOrdersRepository>();
builder.Services.AddScoped<IAuthRepository, MySqlAuthRepository>();
builder.Services.AddScoped<IHomeRepository, MySqlHomeRepository>();
builder.Services.AddScoped<IPaymentsRepository, MySqlPaymentsRepository>();
builder.Services.AddScoped<IProfileRepository, MySqlProfileRepository>();
builder.Services.AddScoped<IClientRepository, MySqlClientRepository>();
builder.Services.AddSingleton<IProviderRepository, InMemoryProviderRepository>();
builder.Services.AddSingleton<ICompanyRepository, InMemoryCompanyRepository>();

var app = builder.Build();

await DatabaseMigrator.ApplyMigrationsAsync(app.Services, app.Logger);

if (builder.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("ENABLE_SWAGGER"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapGet("/", () => Results.Ok(new { service = "FuelTrack.Api", status = "running" }))
    .AllowAnonymous();

app.Run();
