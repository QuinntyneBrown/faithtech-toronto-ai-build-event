namespace FaithTechTorontoAiBuildEvent.Domain.Events;

public sealed class LogoAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public byte[] Bytes { get; set; } = [];
    public string MediaType { get; set; } = "image/png";
    public int Width { get; set; }
    public int Height { get; set; }
}
