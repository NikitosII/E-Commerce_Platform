using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Order;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetOrdersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(Guid id, CancellationToken cancellationToken = default);
}
