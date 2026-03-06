using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetInStockAsync(CancellationToken cancellationToken = default);
}
