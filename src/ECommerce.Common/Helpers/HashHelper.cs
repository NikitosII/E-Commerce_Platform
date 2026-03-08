using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Common.Helpers;

public static class HashHelper
{
    public static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string GenerateCorrelationId() => Guid.NewGuid().ToString("N");
}
