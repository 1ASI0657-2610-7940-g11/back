using FuelTrack.Api.Features.Orders.Domain;
using FuelTrack.Api.Infrastructure.Data;
using FuelTrack.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FuelTrack.Api.Features.Orders.Data;

public sealed class MySqlOrdersRepository : IOrdersRepository
{
    private readonly FuelTrackDbContext _db;

    public MySqlOrdersRepository(FuelTrackDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<OrderSummary>> GetOrdersAsync(
        string userId,
        OrderStatus? status = null)
    {
        var query = UserOrders(userId);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        return await query.OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => ToSummary(x))
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderSummary>> GetOrderHistoryAsync(
        string userId,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = UserOrders(userId);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAtUtc >= fromDate.Value.Date);
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAtUtc < toDate.Value.Date.AddDays(1));

        return await query.OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => ToSummary(x))
            .ToListAsync();
    }

    public async Task<OrderDetail?> GetOrderDetailAsync(string userId, string id)
    {
        var entity = await UserOrders(userId)
            .SingleOrDefaultAsync(x => x.Id == id);
        return entity is null ? null : ToDetail(entity);
    }

    public async Task<OrderDetail?> GetOrderByCodeAsync(string userId, string code)
    {
        var entity = await UserOrders(userId)
            .SingleOrDefaultAsync(x => x.Code == code || x.Id == code);
        return entity is null ? null : ToDetail(entity);
    }

    public async Task<OrderDetail> CreateOrderAsync(
        string userId,
        NewOrderRequest request)
    {
        var now = DateTime.UtcNow;
        var entity = new OrderEntity
        {
            UserId = userId,
            Code = $"FT-{now:yyyy}-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
            Status = OrderStatus.Scheduled,
            Product = request.FuelType.Trim(),
            QuantityGallons = request.QuantityGallons,
            CreatedAtUtc = now,
            Eta = "Entrega pendiente de programación",
            Plant = "Por asignar",
            Address = request.Address.Trim(),
            TimeWindow = request.TimeWindow.Trim(),
            Notes = request.Notes?.Trim()
        };
        _db.Orders.Add(entity);
        await _db.SaveChangesAsync();
        return ToDetail(entity);
    }

    public async Task<OrderDetail?> UpdateOrderStatusAsync(
        string userId,
        string id,
        UpdateOrderStatusRequest request)
    {
        var entity = await UserOrders(userId).SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return null;

        entity.Status = request.Status;
        entity.LastStatusComment = request.Comment?.Trim();
        entity.Eta = request.Status switch
        {
            OrderStatus.Created => "Pedido creado",
            OrderStatus.Scheduled => "Pedido programado",
            OrderStatus.OnRoute => "Pedido en ruta",
            OrderStatus.Delivered => $"Entregado el {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC",
            OrderStatus.Cancelled => $"Cancelado el {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC",
            _ => entity.Eta
        };
        await _db.SaveChangesAsync();
        return ToDetail(entity);
    }

    public async Task<OrderDetail?> AssignVehicleAsync(
        string userId,
        string id,
        AssignVehicleRequest request)
    {
        var entity = await UserOrders(userId).SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return null;

        entity.VehicleId = request.VehicleId.Trim();
        entity.VehiclePlate = request.VehiclePlate?.Trim();
        entity.DriverName = request.DriverName?.Trim();
        if (entity.Status == OrderStatus.Created)
        {
            entity.Status = OrderStatus.Scheduled;
            entity.Eta = "Vehículo asignado. Entrega pendiente de programación";
        }
        await _db.SaveChangesAsync();
        return ToDetail(entity);
    }

    private IQueryable<OrderEntity> UserOrders(string userId)
    {
        return _db.Orders.Where(x => x.UserId == userId);
    }

    private static OrderSummary ToSummary(OrderEntity order)
    {
        return new OrderSummary
        {
            Id = order.Id,
            Code = order.Code,
            Status = order.Status,
            ScheduledAt = order.Eta,
            PlantName = order.Plant,
            FuelType = order.Product,
            QuantityGallons = order.QuantityGallons,
            VehiclePlate = order.VehiclePlate
        };
    }

    private static OrderDetail ToDetail(OrderEntity order)
    {
        return new OrderDetail
        {
            Id = order.Id,
            Code = order.Code,
            Status = order.Status,
            Product = order.Product,
            QuantityGallons = order.QuantityGallons,
            CreatedAt = $"Creado el {order.CreatedAtUtc:dd/MM/yyyy HH:mm} UTC",
            CreatedDate = order.CreatedAtUtc,
            Eta = order.Eta,
            Plant = order.Plant,
            Address = order.Address,
            TimeWindow = order.TimeWindow,
            PaymentMethod = order.PaymentMethod,
            Amount = order.Amount.HasValue ? (double)order.Amount.Value : null,
            VehicleId = order.VehicleId,
            VehiclePlate = order.VehiclePlate,
            DriverName = order.DriverName,
            LastStatusComment = order.LastStatusComment
        };
    }
}
