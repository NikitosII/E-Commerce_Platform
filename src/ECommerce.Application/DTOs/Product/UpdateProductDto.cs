using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Product;

public class UpdateProductDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    [Required]
    public Guid CategoryId { get; set; }
}
