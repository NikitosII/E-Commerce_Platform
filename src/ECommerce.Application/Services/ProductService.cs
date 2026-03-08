using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces;
using ECommerce.Common.Constants;
using ECommerce.Common.Helpers;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ICacheService cache, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }
    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        StockQuantity = p.StockQuantity,
        ImageUrl = p.ImageUrl,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty
    };

    public async Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        var active = all.Where(p => !p.IsDeleted).ToList();

        var normalizedPage = PaginationHelper.NormalizePage(page);
        var normalizedSize = PaginationHelper.NormalizePageSize(pageSize);
        var items = active
            .Skip(PaginationHelper.CalculateSkip(normalizedPage, normalizedSize))
            .Take(normalizedSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = active.Count,
            Page = normalizedPage,
            PageSize = normalizedSize,
            TotalPages = PaginationHelper.CalculateTotalPages(active.Count, normalizedSize)
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{AppConstants.Cache.ProductPrefix}{id}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            return product is null || product.IsDeleted ? null! : MapToDto(product);
        }, TimeSpan.FromMinutes(AppConstants.Cache.DefaultExpirationMinutes), cancellationToken);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId, cancellationToken);
        return products.Where(p => !p.IsDeleted).Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.SearchAsync(searchTerm, cancellationToken);
        return products.Where(p => !p.IsDeleted).Select(MapToDto);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var categoryExists = await _unitOfWork.Categories.ExistsAsync(dto.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new KeyNotFoundException(string.Format(ErrorMessages.NotFound, nameof(Category), dto.CategoryId));

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);
        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(string.Format(ErrorMessages.NotFound, nameof(Product), id));

        if (product.CategoryId != dto.CategoryId)
        {
            var categoryExists = await _unitOfWork.Categories.ExistsAsync(dto.CategoryId, cancellationToken);
            if (!categoryExists)
                throw new KeyNotFoundException(string.Format(ErrorMessages.NotFound, nameof(Category), dto.CategoryId));
        }

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.ImageUrl = dto.ImageUrl;
        product.CategoryId = dto.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync($"{AppConstants.Cache.ProductPrefix}{id}", cancellationToken);

        return MapToDto(product);
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(string.Format(ErrorMessages.NotFound, nameof(Product), id));

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync($"{AppConstants.Cache.ProductPrefix}{id}", cancellationToken);
    }

}
