using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ECommerceDbContext context) : base(context) { }

    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbSet.Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        await _dbSet.Include(p => p.Category)
                    .Where(p => p.CategoryId == categoryId)
                    .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default) =>
        await _dbSet.Include(p => p.Category)
                    .Where(p => EF.Functions.ILike(p.Name, $"%{searchTerm}%") ||
                                (p.Description != null && EF.Functions.ILike(p.Description, $"%{searchTerm}%")))
                    .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Product>> GetInStockAsync(CancellationToken cancellationToken = default) =>
        await _dbSet.Include(p => p.Category)
                    .Where(p => p.StockQuantity > 0)
                    .ToListAsync(cancellationToken);
}
