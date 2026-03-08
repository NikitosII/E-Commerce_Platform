using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ECommerceDbContext context) : base(context) { }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default) =>
        await _dbSet.Include(c => c.SubCategories)
                    .Where(c => c.ParentCategoryId == null)
                    .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default) =>
        await _dbSet.Where(c => c.ParentCategoryId == parentId)
                    .ToListAsync(cancellationToken);
}
