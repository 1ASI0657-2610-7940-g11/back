using FuelTrack.Api.Features.Orders.Domain;

namespace FuelTrack.Api.Infrastructure.Data.Entities;

public sealed class OrderEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = default!;
    public string Code { get; set; } = default!;
    public OrderStatus Status { get; set; }
    public string Product { get; set; } = default!;
    public int QuantityGallons { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Eta { get; set; } = default!;
    public string Plant { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string TimeWindow { get; set; } = default!;
    public string? Notes { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal? Amount { get; set; }
    public string? VehicleId { get; set; }
    public string? VehiclePlate { get; set; }
    public string? DriverName { get; set; }
    public string? LastStatusComment { get; set; }
    public UserEntity User { get; set; } = default!;
}
