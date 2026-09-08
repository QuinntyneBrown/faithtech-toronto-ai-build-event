namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class Program
{
    public static Task<int> Main(string[] args) => new OperatorRootCommand().InvokeAsync(args);
}
