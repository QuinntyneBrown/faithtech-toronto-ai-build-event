namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public interface IEntryReceiptSecretService
{
    string CreateSecret();
    byte[] Digest(string secret);
    bool Matches(byte[] expectedDigest, string secret);
}
