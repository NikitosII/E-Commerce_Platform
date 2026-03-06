namespace ECommerce.Common.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string? value) =>
        string.IsNullOrWhiteSpace(value);

    public static string Truncate(this string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    public static string ToSlug(this string value) =>
        value.ToLowerInvariant()
             .Replace(" ", "-")
             .Replace("_", "-");
}
