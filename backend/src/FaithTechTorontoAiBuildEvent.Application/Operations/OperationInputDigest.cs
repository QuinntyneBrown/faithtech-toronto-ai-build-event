using System.Security.Cryptography;
using System.Text;

namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public static class OperationInputDigest
{
    public static byte[] Create(params string?[] values)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var value in values)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            hash.AppendData(BitConverter.GetBytes(bytes.Length));
            hash.AppendData(bytes);
        }
        return hash.GetHashAndReset();
    }
}
