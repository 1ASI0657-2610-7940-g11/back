using FuelTrack.Api.Features.Client.Domain;
using FuelTrack.Api.Features.Orders.Domain;
using FuelTrack.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FuelTrack.Api.Features.Client.Data;

public sealed class MySqlClientRepository : IClientRepository
{
    private readonly FuelTrackDbContext _db;

    public MySqlClientRepository(FuelTrackDbContext db)
    {
        _db = db;
    }

    public async Task<ClientKpis> GetClientKpisAsync(string userId)
    {
        var orders = await _db.Orders
            .Where(x => x.UserId == userId)
            .ToListAsync();
        var totalSpent = orders.Sum(x => x.Amount ?? 0m);

        return new ClientKpis
        {
            TotalOrders = orders.Count,
            ActiveOrders = orders.Count(x =>
                x.Status != OrderStatus.Delivered && x.Status != OrderStatus.Cancelled),
            DeliveredOrders = orders.Count(x => x.Status == OrderStatus.Delivered),
            PendingOrders = orders.Count(x =>
                x.Status == OrderStatus.Created || x.Status == OrderStatus.Scheduled),
            CancelledOrders = orders.Count(x => x.Status == OrderStatus.Cancelled),
            TotalGallons = orders.Sum(x => x.QuantityGallons),
            TotalSpent = totalSpent,
            AverageOrderAmount = orders.Count == 0 ? 0 : totalSpent / orders.Count,
            LastOrderDate = orders.OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefault()?.CreatedAtUtc.ToString("yyyy-MM-dd"),
            NextDeliveryDate = null,
            OrdersByStatus = orders.GroupBy(x => x.Status)
                .Select(x => new ClientKpiStatusItem
                {
                    Status = x.Key.ToString(),
                    Count = x.Count()
                })
                .ToList()
        };
    }
}
