using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(AppDbContext db,
        ILogger<OrdersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() =>
        User.IsInRole("Admin");

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetAll()
    {
        _logger.LogInformation("Fetching orders for UserID: {UserId}", GetUserId());

        var query = _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .AsQueryable();

        if (!IsAdmin())
            query = query.Where(o => o.UserId == GetUserId());

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => MapToDto(o))
            .ToListAsync();

        _logger.LogInformation("Fetched {Count} orders.", orders.Count);

        return Ok(new ApiResponse<List<OrderResponseDto>>(
            true, "Orders fetched.", orders));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetById(int id)
    {
        _logger.LogInformation("Fetching order with ID: {Id}", id);

        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            _logger.LogWarning("Order not found with ID: {Id}", id);
            return NotFound(new ApiResponse<OrderResponseDto>(
                false, "Order not found."));
        }

        if (!IsAdmin() && order.UserId != GetUserId())
        {
            _logger.LogWarning("Unauthorized access to order ID: {Id}", id);
            return Forbid();
        }

        return Ok(new ApiResponse<OrderResponseDto>(
            true, "Order found.", MapToDto(order)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> Create(
        CreateOrderDto dto)
    {
        _logger.LogInformation(
            "Creating order for UserID: {UserId}, Items: {Count}",
            GetUserId(), dto.Items.Count);

        if (!dto.Items.Any())
            return BadRequest(new ApiResponse<OrderResponseDto>(
                false, "Order must have at least one item."));

        var order = new Order
        {
            UserId = GetUserId(),
            ShippingAddress = dto.ShippingAddress,
            OrderItems = new List<OrderItem>()
        };

        decimal total = 0;

        foreach (var item in dto.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);

            if (product == null || !product.IsActive)
            {
                _logger.LogWarning("Product not found: {ProductId}", item.ProductId);
                return BadRequest(new ApiResponse<OrderResponseDto>(
                    false, $"Product {item.ProductId} not found."));
            }

            if (product.Stock < item.Quantity)
            {
                _logger.LogWarning(
                    "Insufficient stock for product: {ProductId}, " +
                    "Available: {Stock}, Requested: {Quantity}",
                    item.ProductId, product.Stock, item.Quantity);
                return BadRequest(new ApiResponse<OrderResponseDto>(
                    false, $"Insufficient stock for '{product.Name}'. " +
                    $"Available: {product.Stock}"));
            }

            product.Stock -= item.Quantity;
            total += product.Price * item.Quantity;

            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        order.TotalAmount = total;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Order created with ID: {OrderId}, Total: {Total}",
            order.Id, order.TotalAmount);

        var created = await _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstAsync(o => o.Id == order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            new ApiResponse<OrderResponseDto>(
                true, "Order placed successfully.", MapToDto(created)));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateStatus(
        int id, UpdateOrderStatusDto dto)
    {
        _logger.LogInformation(
            "Updating order status - ID: {Id}, Status: {Status}",
            id, dto.Status);

        var validStatuses = new[]
        {
            "Pending", "Processing", "Shipped", "Delivered", "Cancelled"
        };

        if (!validStatuses.Contains(dto.Status))
            return BadRequest(new ApiResponse<OrderResponseDto>(
                false, $"Invalid status. Valid: {string.Join(", ", validStatuses)}"));

        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            _logger.LogWarning("Order not found with ID: {Id}", id);
            return NotFound(new ApiResponse<OrderResponseDto>(
                false, "Order not found."));
        }

        order.Status = dto.Status;

        if (dto.Status == "Delivered")
            order.DeliveredAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Order status updated - ID: {Id}, Status: {Status}",
            id, dto.Status);

        return Ok(new ApiResponse<OrderResponseDto>(
            true, "Order status updated.", MapToDto(order)));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(int id)
    {
        _logger.LogInformation("Cancelling order with ID: {Id}", id);

        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            _logger.LogWarning("Order not found with ID: {Id}", id);
            return NotFound(new ApiResponse<object>(
                false, "Order not found."));
        }

        if (!IsAdmin() && order.UserId != GetUserId())
        {
            _logger.LogWarning("Unauthorized cancel attempt for order ID: {Id}", id);
            return Forbid();
        }

        if (order.Status != "Pending")
            return BadRequest(new ApiResponse<object>(
                false, "Only Pending orders can be cancelled."));

        foreach (var item in order.OrderItems)
            item.Product.Stock += item.Quantity;

        order.Status = "Cancelled";
        await _db.SaveChangesAsync();

        _logger.LogInformation("Order cancelled - ID: {Id}, Stock restored.", id);

        return Ok(new ApiResponse<object>(
            true, "Order cancelled and stock restored."));
    }

    private static OrderResponseDto MapToDto(Order o) => new(
        o.Id, o.UserId, o.Status, o.TotalAmount,
        o.ShippingAddress, o.OrderDate,
        o.OrderItems.Select(oi => new OrderItemResponseDto(
            oi.ProductId,
            oi.Product?.Name ?? "",
            oi.Quantity,
            oi.UnitPrice,
            oi.UnitPrice * oi.Quantity
        )).ToList()
    );
}