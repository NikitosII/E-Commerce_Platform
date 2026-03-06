using ECommerce.Common.Helpers;

namespace ECommerce.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var skip = PaginationHelper.CalculateSkip(page, pageSize);
        var take = PaginationHelper.NormalizePageSize(pageSize);
        return query.Skip(skip).Take(take);
    }
}
