using System.Text;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

internal static class SecretInput
{
    public static string ReadPassword()
    {
        if (Console.IsInputRedirected) return Console.ReadLine() ?? throw new InvalidOperationException("Password is required.");
        Console.Write("Password: ");
        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return password.ToString(); }
            if (key.Key == ConsoleKey.Backspace && password.Length > 0) password.Length--;
            else if (!char.IsControl(key.KeyChar)) password.Append(key.KeyChar);
        }
    }
}
