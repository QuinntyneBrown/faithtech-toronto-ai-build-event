using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Operations;

public sealed class ProtectedOperatorFiles
{
    private readonly string directory;
    public ProtectedOperatorFiles(string directory)
    {
        this.directory = Path.GetFullPath(directory);
        var info = Directory.CreateDirectory(this.directory);
        if ((info.Attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("Operator directory cannot be a link.");
        if (OperatingSystem.IsWindows()) {
            var security = new DirectorySecurity();
            security.SetAccessRuleProtection(true, false);
            security.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!, FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            info.SetAccessControl(security);
        } else File.SetUnixFileMode(this.directory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    }

    public void Write<T>(string name, T value)
    {
        var destination = FilePath(name); var temporary = FilePath(Guid.NewGuid().ToString("N"));
        var plain = JsonSerializer.SerializeToUtf8Bytes(value);
        try {
            var encrypted = Protect(plain);
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                file.Write(encrypted); file.Flush(true);
            }
            File.Move(temporary, destination, true);
        } finally { CryptographicOperations.ZeroMemory(plain); if (File.Exists(temporary)) File.Delete(temporary); }
    }
    public T Read<T>(string name)
    {
        var plain = Unprotect(File.ReadAllBytes(FilePath(name)));
        try { return JsonSerializer.Deserialize<T>(plain) ?? throw new InvalidOperationException("Invalid operator artifact."); }
        finally { CryptographicOperations.ZeroMemory(plain); }
    }
    public bool Exists(string name) => File.Exists(FilePath(name));
    public FileStream Lock(Guid operation) => new(FilePath($"{operation:N}.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    public void Journal(Guid operation, object entry)
    {
        using var file = new FileStream(FilePath($"{operation:N}.jsonl"), FileMode.Append, FileAccess.Write, FileShare.Read);
        file.Write(JsonSerializer.SerializeToUtf8Bytes(entry)); file.WriteByte(10); file.Flush(true);
    }
    private string FilePath(string name) => name.Length > 0 && name.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '.') && !name.Contains("..", StringComparison.Ordinal)
        ? Path.Combine(directory, name) : throw new ArgumentException("Invalid artifact name.");
    private byte[] Protect(byte[] plain)
    {
        if (OperatingSystem.IsWindows()) return ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
        var result = new byte[28 + plain.Length]; RandomNumberGenerator.Fill(result.AsSpan(0, 12));
        using var aes = new AesGcm(Key(), 16); aes.Encrypt(result.AsSpan(0, 12), plain, result.AsSpan(28), result.AsSpan(12, 16)); return result;
    }
    private byte[] Unprotect(byte[] encrypted)
    {
        if (OperatingSystem.IsWindows()) return ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        if (encrypted.Length < 28) throw new CryptographicException();
        var plain = new byte[encrypted.Length - 28];
        using var aes = new AesGcm(Key(), 16); aes.Decrypt(encrypted.AsSpan(0, 12), encrypted.AsSpan(28), encrypted.AsSpan(12, 16), plain); return plain;
    }
    private byte[] Key()
    {
        var path = FilePath("key");
        if (!File.Exists(path)) {
            try { using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None); file.Write(RandomNumberGenerator.GetBytes(32)); file.Flush(true); }
            catch (IOException) when (File.Exists(path)) { }
        }
        return File.ReadAllBytes(path);
    }
}
