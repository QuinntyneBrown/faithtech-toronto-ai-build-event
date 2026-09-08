namespace FaithTechTorontoAiBuildEvent.Application.Events;

public interface ILogoDecoder
{
    LogoContent Decode(byte[] bytes, string mediaType);
}
