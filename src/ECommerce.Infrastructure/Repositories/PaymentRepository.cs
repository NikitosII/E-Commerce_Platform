using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ECommerceDbContext context) : base(context) { }

    public async Task<Payment?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        await _dbSet.FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default) =>
        await _dbSet.FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
}
