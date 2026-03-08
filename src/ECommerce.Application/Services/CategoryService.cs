using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Interfaces;
using ECommerce.Common.Constants;
using ECommerce.Common.Extensions;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork unitOfWork, ICacheService cache, ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        return categories.Where(c => !c.IsDeleted && c.ParentCategoryId == null)
                         .Select(MapToDto);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{AppConstants.Cache.CategoryPrefix}{id}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
            return category is null || category.IsDeleted ? null! : MapToDto(category);
        }, TimeSpan.FromMinutes(AppConstants.Cache.DefaultExpirationMinutes), cancellationToken);
    }

    public async Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var subs = await _unitOfWork.Categories.GetSubCategoriesAsync(parentId, cancellationToken);
        return subs.Where(c => !c.IsDeleted).Select(MapToDto);
    }

    public async Task<CategoryDto> CreateCategoryAsync(
        string name,
        string? description,
        Guid? parentId,
        CancellationToken cancellationToken = default)
    {
        if (name.IsNullOrWhiteSpace())
            throw new InvalidOperationException(string.Format(ErrorMessages.InvalidOperation, "category name cannot be empty"));

        if (parentId.HasValue)
        {
            var parentExists = await _unitOfWork.Categories.ExistsAsync(parentId.Value, cancellationToken);
            if (!parentExists)
                throw new KeyNotFoundException(string.Format(ErrorMessages.NotFound, nameof(Category), parentId.Value));
        }

        var category = new Category
        {
            Name = name,
            Description = description,
            ParentCategoryId = parentId
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category created: {CategoryId} - {CategoryName}", category.Id, category.Name);
        return MapToDto(category);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(string.Format(ErrorMessages.NotFound, nameof(Category), id));

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync($"{AppConstants.Cache.CategoryPrefix}{id}", cancellationToken);
    }

    private static CategoryDto MapToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        ParentCategoryId = c.ParentCategoryId,
        SubCategories = c.SubCategories
            .Where(s => !s.IsDeleted)
            .Select(MapToDto)
            .ToList()
    };
}
