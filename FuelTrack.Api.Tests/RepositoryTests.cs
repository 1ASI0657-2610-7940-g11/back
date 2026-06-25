using FuelTrack.Api.Features.Auth.Data;
using FuelTrack.Api.Features.Auth.Domain;
using FuelTrack.Api.Features.Orders.Data;
using FuelTrack.Api.Features.Orders.Domain;
using FuelTrack.Api.Features.Payments.Data;
using FuelTrack.Api.Features.Payments.Domain;
using FuelTrack.Api.Features.Profile.Data;
using FuelTrack.Api.Infrastructure.Auth;
using FuelTrack.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FuelTrack.Api.Tests;

public sealed class RepositoryTests
{
    [Fact]
    public async Task RegistrationHashesPasswordAndCreatesEmptyProfile()
    {
        await using var db = CreateDatabase();
        var repository = CreateAuthRepository(db);

        var result = await repository.RegisterAsync(
            new RegisterRequest("Ana Pérez", "ANA@EXAMPLE.COM", "correct-password"));

        var stored = await db.Users.Include(x => x.Profile).SingleAsync();
        Assert.NotEqual("correct-password", stored.PasswordHash);
        Assert.Equal("ana@example.com", stored.Email);
        Assert.Equal("", stored.Profile.CompanyName);
        Assert.NotEmpty(result.Token);

        var login = await repository.LoginAsync(
            new LoginRequest("ana@example.com", "correct-password"));
        Assert.NotNull(login);
    }

    [Fact]
    public async Task OrdersAreIsolatedByUser()
    {
        await using var db = CreateDatabase();
        var repository = new MySqlOrdersRepository(db);
        var userOne = await RegisterUser(db, "one@example.com");
        var userTwo = await RegisterUser(db, "two@example.com");

        await repository.CreateOrderAsync(userOne, NewOrder());
        await repository.CreateOrderAsync(userTwo, NewOrder());

        var userOneOrders = await repository.GetOrdersAsync(userOne);
        var userTwoOrders = await repository.GetOrdersAsync(userTwo);
        Assert.Single(userOneOrders);
        Assert.Single(userTwoOrders);
        Assert.NotEqual(userOneOrders.Single().Id, userTwoOrders.Single().Id);
    }

    [Fact]
    public async Task PaymentMethodStoresOnlyLastFourDigits()
    {
        await using var db = CreateDatabase();
        var userId = await RegisterUser(db, "cards@example.com");
        var repository = new MySqlPaymentsRepository(db);

        await repository.AddPaymentMethodAsync(userId, new NewPaymentMethodRequest
        {
            Brand = "Visa",
            CardNumber = "4111111111111234",
            Holder = "Empresa Demo",
            Expires = "12/30"
        });

        var stored = await db.PaymentMethods.SingleAsync();
        Assert.Equal("1234", stored.Last4);
        Assert.DoesNotContain("411111111111", stored.Last4);
    }

    [Fact]
    public async Task AvatarIsPersistedForItsOwner()
    {
        await using var db = CreateDatabase();
        var userId = await RegisterUser(db, "avatar@example.com");
        var repository = new MySqlProfileRepository(db);
        var content = new byte[] { 1, 2, 3, 4 };

        var profile = await repository.UpdateAvatarAsync(userId, content, "image/png");
        var avatar = await repository.GetAvatarAsync(userId);

        Assert.Equal("/api/profile/avatar", profile.AvatarUrl);
        Assert.NotNull(avatar);
        Assert.Equal(content, avatar.Content);
        Assert.Equal("image/png", avatar.ContentType);
    }

    private static FuelTrackDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<FuelTrackDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new FuelTrackDbContext(options);
    }

    private static MySqlAuthRepository CreateAuthRepository(FuelTrackDbContext db)
    {
        var jwt = new JwtOptions(
            new string('x', 64),
            "FuelTrack.Api",
            "FuelTrack.Web",
            120);
        return new MySqlAuthRepository(
            db,
            new PasswordHashService(),
            new TokenService(jwt));
    }

    private static async Task<string> RegisterUser(
        FuelTrackDbContext db,
        string email)
    {
        var repository = CreateAuthRepository(db);
        var result = await repository.RegisterAsync(
            new RegisterRequest("Test User", email, "correct-password"));
        return result.User.Id;
    }

    private static NewOrderRequest NewOrder()
    {
        return new NewOrderRequest
        {
            FuelType = "Diesel B5",
            QuantityGallons = 1000,
            Address = "Av. Prueba 123",
            TimeWindow = "Mañana",
            Notes = "Prueba"
        };
    }
}
