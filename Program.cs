using FuelTrack.Api.Features.Orders.Data;
using FuelTrack.Api.Features.Orders.Domain;
using FuelTrack.Api.Features.Auth.Data;
using FuelTrack.Api.Features.Auth.Domain;
using FuelTrack.Api.Features.Home.Data;
using FuelTrack.Api.Features.Home.Domain;
using FuelTrack.Api.Features.Payments.Domain;
using FuelTrack.Api.Features.Payments.Data;
using FuelTrack.Api.Features.Profile.Domain;
using FuelTrack.Api.Features.Profile.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// NOTA: Se eliminó builder.WebHost.UseUrls(...) 
// Docker y Render asignarán el puerto automáticamente (8080).

// Registramos servicios MVC / Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Usar el nombre completo del tipo como schemaId para evitar colisiones
    c.CustomSchemaIds(type => type.FullName?.Replace('.', '_'));
});

// Inyectamos repositorios (por ahora solo Orders, in-memory)
builder.Services.AddSingleton<IOrdersRepository, InMemoryOrdersRepository>();
builder.Services.AddSingleton<IAuthRepository, InMemoryAuthRepository>();
builder.Services.AddSingleton<IHomeRepository, InMemoryHomeRepository>();
builder.Services.AddSingleton<IPaymentsRepository, InMemoryPaymentsRepository>();
builder.Services.AddSingleton<IProfileRepository, InMemoryProfileRepository>();

var app = builder.Build();

// --- CORRECCIÓN: Swagger habilitado fuera del IF ---
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseCors("LocalFrontend");

// app.UseHttpsRedirection(); // A veces es mejor comentar esto en Render si hay problemas de redirección, pero dejémoslo por ahora.
app.UseAuthorization();

// Usar controllers (OrdersController, etc.)
app.MapControllers();

// --- TRUCO: Redirigir la página de inicio a Swagger ---
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
