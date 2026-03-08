using ECommerce.Common.Constants;

namespace ECommerce.Common.Helpers;

public static class PaginationHelper
{
    public static int NormalizePage(int page) => Math.Max(1, page);

    public static int NormalizePageSize(int pageSize) =>
        Math.Clamp(pageSize, 1, AppConstants.Pagination.MaxPageSize);

    public static int CalculateSkip(int page, int pageSize) =>
        (NormalizePage(page) - 1) * NormalizePageSize(pageSize);

    public static int CalculateTotalPages(int totalCount, int pageSize) =>
        (int)Math.Ceiling((double)totalCount / NormalizePageSize(pageSize));
}
