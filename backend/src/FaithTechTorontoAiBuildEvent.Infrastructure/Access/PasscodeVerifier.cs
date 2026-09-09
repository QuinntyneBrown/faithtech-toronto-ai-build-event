using System.Security.Cryptography;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class PasscodeVerifier
{
    private static readonly byte[] DomainBytes = System.Text.Encoding.ASCII.GetBytes("FaithTech.Companion.Passcode.v1:");

    public (byte[] Salt, byte[] Verifier) Create(string passcode)
    {
        Validate(passcode);
        var salt = RandomNumberGenerator.GetBytes(32);
        return (salt, Hash(salt, passcode));
    }

    public bool Verify(byte[] salt, byte[] verifier, string passcode)
    {
        Validate(passcode);
        return CryptographicOperations.FixedTimeEquals(verifier, Hash(salt, passcode));
    }

    private static byte[] Hash(byte[] salt, string passcode)
    {
        var digits = System.Text.Encoding.ASCII.GetBytes(passcode);
        var bytes = new byte[DomainBytes.Length + salt.Length + digits.Length];
        Buffer.BlockCopy(DomainBytes, 0, bytes, 0, DomainBytes.Length);
        Buffer.BlockCopy(salt, 0, bytes, DomainBytes.Length, salt.Length);
        Buffer.BlockCopy(digits, 0, bytes, DomainBytes.Length + salt.Length, digits.Length);
        return SHA512.HashData(bytes);
    }

    private static void Validate(string passcode)
    {
        if (passcode.Length != 4 || passcode.Any(character => character is < '0' or > '9'))
        {
            throw new ArgumentException("Passcode must contain exactly four ASCII digits.", nameof(passcode));
        }
    }
}
