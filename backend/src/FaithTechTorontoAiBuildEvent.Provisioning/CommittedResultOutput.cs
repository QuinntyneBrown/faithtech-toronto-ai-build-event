using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

internal static class CommittedResultOutput
{
    public static void Write(string[] lines)
    {
        try
        {
            if (!Console.IsOutputRedirected)
            {
                foreach (var line in lines) Console.WriteLine(line);
                return;
            }
            Console.Out.Flush();
            using var handle = new SafeFileHandle(OperatingSystem.IsWindows() ? GetStdHandle(-11) : new IntPtr(1), ownsHandle: false);
            using var stream = new FileStream(handle, FileAccess.Write);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            foreach (var line in lines) writer.WriteLine(line);
            writer.Flush();
        }
        catch (IOException) { throw new CommittedOutputException(); }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int standardHandle);
}
