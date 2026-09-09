namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class PasscodeInput
{
    public static string? ReadMasked()
    {
        Console.Write("New administrator passcode: ");
        var characters = new List<char>();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace && characters.Count > 0)
            {
                characters.RemoveAt(characters.Count - 1);
                continue;
            }
            if (!char.IsControl(key.KeyChar)) characters.Add(key.KeyChar);
        }
        Console.WriteLine();
        return new string(characters.ToArray());
    }
}
