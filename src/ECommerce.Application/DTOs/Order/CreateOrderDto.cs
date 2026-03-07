using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Order;

public class CreateOrderDto
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required, MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = new();

    [MaxLength(500)]
    public string? ShippingAddress { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class CreateOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}
