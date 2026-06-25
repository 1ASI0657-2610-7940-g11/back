using FuelTrack.Api.Features.Orders.Domain;
using FuelTrack.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FuelTrack.Api.Features.Orders.Api;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrdersRepository _repository;

    public OrdersController(IOrdersRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderSummary>>> GetOrders(
        [FromQuery] OrderStatus? status = null)
    {
        return Ok(await _repository.GetOrdersAsync(User.GetRequiredUserId(), status));
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<OrderSummary>>> GetOrderHistory(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _repository.GetOrderHistoryAsync(
            User.GetRequiredUserId(), status, fromDate, toDate));
    }

    [HttpGet("code/{code}")]
    public async Task<ActionResult<OrderDetail>> GetOrderByCode(string code)
    {
        var order = await _repository.GetOrderByCodeAsync(User.GetRequiredUserId(), code);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetail>> GetOrderDetail(string id)
    {
        var order = await _repository.GetOrderDetailAsync(User.GetRequiredUserId(), id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDetail>> CreateOrder(
        [FromBody] NewOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FuelType)
            || request.QuantityGallons <= 0
            || string.IsNullOrWhiteSpace(request.Address)
            || string.IsNullOrWhiteSpace(request.TimeWindow))
        {
            return BadRequest(new
            {
                message = "Completa combustible, cantidad, dirección y horario."
            });
        }

        var created = await _repository.CreateOrderAsync(
            User.GetRequiredUserId(), request);
        return CreatedAtAction(nameof(GetOrderDetail), new { id = created.Id }, created);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<OrderDetail>> UpdateOrderStatus(
        string id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        var updated = await _repository.UpdateOrderStatusAsync(
            User.GetRequiredUserId(), id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPatch("{id}/vehicle")]
    public async Task<ActionResult<OrderDetail>> AssignVehicle(
        string id,
        [FromBody] AssignVehicleRequest request)
    {
        var updated = await _repository.AssignVehicleAsync(
            User.GetRequiredUserId(), id, request);
        return updated is null ? NotFound() : Ok(updated);
    }
}
