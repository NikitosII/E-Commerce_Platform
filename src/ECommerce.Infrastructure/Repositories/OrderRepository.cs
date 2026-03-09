using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ECommerceDbContext context) : base(context) { }

    public override async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet.Include(o => o.Items).ThenInclude(i => i.Product)
                    .Include(o => o.Customer)
                    .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        await _dbSet.Include(o => o.Items).ThenInclude(i => i.Product)
                    .Include(o => o.Customer)
                    .Where(o => o.CustomerId == customerId)
                    .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default) =>
        await _dbSet.Include(o => o.Customer)
                    .Where(o => o.Status == status)
                    .ToListAsync(cancellationToken);

    public async Task<Order?> GetWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        await _dbSet.Include(o => o.Items).ThenInclude(i => i.Product)
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
}
