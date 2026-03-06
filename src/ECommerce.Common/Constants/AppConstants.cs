namespace ECommerce.Common.Constants;

public static class AppConstants
{
    public static class Cache
    {
        public const int DefaultExpirationMinutes = 30;
        public const string ProductPrefix = "product:";
        public const string CategoryPrefix = "category:";
        public const string OrderPrefix = "order:";
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
    }

    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-ID";
        public const string ApiVersion = "X-Api-Version";
    }
}
