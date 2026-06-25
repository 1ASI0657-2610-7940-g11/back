using FuelTrack.Api.Features.Home.Domain;
using FuelTrack.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OrderStatus = FuelTrack.Api.Features.Orders.Domain.OrderStatus;

namespace FuelTrack.Api.Features.Home.Data;

public sealed class MySqlHomeRepository : IHomeRepository
{
    private readonly FuelTrackDbContext _db;

    public MySqlHomeRepository(FuelTrackDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummary> GetDashboardAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _db.Profiles
            .Where(x => x.UserId == userId)
            .Select(x => new { x.CompanyName, HasAvatar = x.AvatarContent != null })
            .SingleAsync(cancellationToken);
        var active = await _db.Orders
            .Where(x => x.UserId == userId
                && x.Status != OrderStatus.Delivered
                && x.Status != OrderStatus.Cancelled)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        var payment = await _db.PaymentHistory
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DateUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return new DashboardSummary
        {
            CompanyName = string.IsNullOrWhiteSpace(profile.CompanyName)
                ? "Cuenta FuelTrack"
                : profile.CompanyName,
            AvatarUrl = profile.HasAvatar ? "/api/profile/avatar" : null,
            ActiveOrder = active is null
                ? null
                : new OrderSummary
                {
                    FuelType = active.Product,
                    QuantityGallons = active.QuantityGallons,
                    Status = active.Status.ToString()
                },
            NextDelivery = active is null
                ? null
                : new DeliverySummary
                {
                    DateTimeText = active.Eta,
                    Location = active.Plant,
                    Status = active.Status.ToString()
                },
            LastPayment = payment is null
                ? null
                : new PaymentSummary
                {
                    AmountText = $"S/ {payment.Amount:N2}",
                    Method = payment.Description,
                    Status = payment.Status
                }
        };
    }
}
