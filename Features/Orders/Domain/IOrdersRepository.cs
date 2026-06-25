namespace FuelTrack.Api.Features.Orders.Domain;

public interface IOrdersRepository
{
    Task<IEnumerable<OrderSummary>> GetOrdersAsync(string userId, OrderStatus? status = null);
    Task<IEnumerable<OrderSummary>> GetOrderHistoryAsync(string userId, OrderStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<OrderDetail?> GetOrderDetailAsync(string userId, string id);
    Task<OrderDetail?> GetOrderByCodeAsync(string userId, string code);
    Task<OrderDetail> CreateOrderAsync(string userId, NewOrderRequest request);
    Task<OrderDetail?> UpdateOrderStatusAsync(string userId, string id, UpdateOrderStatusRequest request);
    Task<OrderDetail?> AssignVehicleAsync(string userId, string id, AssignVehicleRequest request);
}
