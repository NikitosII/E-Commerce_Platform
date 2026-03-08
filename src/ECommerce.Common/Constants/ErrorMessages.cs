namespace ECommerce.Common.Constants;

public static class ErrorMessages
{
    public const string NotFound = "{0} with id '{1}' was not found.";
    public const string AlreadyExists = "{0} already exists.";
    public const string InvalidOperation = "Invalid operation: {0}";
    public const string InsufficientStock = "Insufficient stock for product '{0}'. Available: {1}, Requested: {2}";
    public const string PaymentFailed = "Payment failed: {0}";
    public const string Unauthorized = "You are not authorized to perform this action.";
}
