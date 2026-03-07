using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces;
using ECommerce.Common.Constants;
using ECommerce.Common.Helpers;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }
    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        CustomerId = o.CustomerId,
        CustomerName = o.Customer is not null ? $"{o.Customer.FirstName} {o.Customer.LastName}" : string.Empty,
        Status = o.Status,
        TotalAmount = o.TotalAmount,
        ShippingAddress = o.ShippingAddress,
        CreatedAt = o.CreatedAt,
        Items = o.Items.Select(i => new OrderItemDto
        {
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? string.Empty,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList()
    };

    public async Task<PagedResult<OrderDto>> GetOrdersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
        var active = all.Where(o => !o.IsDeleted).ToList();

        var normalizedPage = PaginationHelper.NormalizePage(page);
        var normalizedSize = PaginationHelper.NormalizePageSize(pageSize);

        var items = active
            .Skip(PaginationHelper.CalculateSkip(normalizedPage, normalizedSize))
            .Take(normalizedSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<OrderDto>
        {
            Items = items,
            TotalCount = active.Count,
            Page = normalizedPage,
            PageSize = normalizedSize,
            TotalPages = PaginationHelper.CalculateTotalPages(active.Count, normalizedSize)
        };
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetWithItemsAsync(id, cancellationToken);
        return order is null || order.IsDeleted ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerAsync(customerId, cancellationToken);
        return orders.Where(o => !o.IsDeleted).Select(MapToDto);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId, cancellationToken)
                ?? throw new KeyNotFoundException(string.Format(ErrorMessages.NotFound, "Customer", dto.CustomerId));

            var order = new Order
            {
                CustomerId = dto.CustomerId,
                ShippingAddress = dto.ShippingAddress,
                Notes = dto.Notes
            };

            foreach (var itemDto in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId, cancellationToken)
                    ?? throw new KeyNotFoundException(string.Format(ErrorMessages.NotFound, "Product", itemDto.ProductId));

                if (product.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException(
                        string.Format(ErrorMessages.InsufficientStock, product.Name, product.StockQuantity, itemDto.Quantity));

                product.StockQuantity -= itemDto.Quantity;
                await _unitOfWork.Products.UpdateAsync(product, cancellationToken);

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price
                });
            }

            order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} created for customer {CustomerId}", order.Id, dto.CustomerId);

            await _emailService.SendOrderConfirmationAsync(customer.Email, order.Id, cancellationToken);

            return MapToDto(order);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(string.Format(ErrorMessages.NotFound, "Order", id));

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(order);
    }

    public async Task CancelOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await UpdateOrderStatusAsync(id, OrderStatus.Cancelled, cancellationToken);
        _logger.LogInformation("Order {OrderId} cancelled", id);
    }
}
