using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SkiaSharp;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventLogoTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-001;L2-040/AC3;L2-044/AC6")]
    [InlineData(SKEncodedImageFormat.Png, "image/png", "venue.png")]
    [InlineData(SKEncodedImageFormat.Jpeg, "image/jpeg", "venue.jpg")]
    [InlineData(SKEncodedImageFormat.Webp, "image/webp", "venue.webp")]
    public async Task Given_a_supported_logo_when_saved_and_retried_then_one_image_is_retained_and_served_privately(SKEncodedImageFormat format, string mediaType, string name)
    {
        using var client = await factory.AdministratorBrowser();
        var original = await Create(client);
        var bytes = Image(format, 3, 2);
        var operationId = Guid.NewGuid().ToString();
        using var response = await Upload(client, original, bytes, mediaType, name, operationId);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, saved.GetProperty("logo").GetProperty("width").GetInt32());
        Assert.NotEqual(original.GetProperty("version").GetString(), saved.GetProperty("version").GetString());
        using var retry = await Upload(client, original, bytes, mediaType, name, operationId);
        Assert.Equal(saved.ToString(), (await retry.Content.ReadFromJsonAsync<JsonElement>()).ToString());
        var path = $"/api/admin/events/{original.GetProperty("id").GetGuid()}/logo";
        using var served = await client.GetAsync(path);
        Assert.Equal("image/png", served.Content.Headers.ContentType?.MediaType);
        Assert.True(served.Headers.CacheControl?.NoStore);
        Assert.Equal("nosniff", served.Headers.GetValues("X-Content-Type-Options").Single());
        using var decoded = SKBitmap.Decode(await served.Content.ReadAsByteArrayAsync());
        Assert.Equal(3, decoded.Width); Assert.Equal(2, decoded.Height);
        using var anonymous = factory.Browser();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(path)).StatusCode);
    }

    private static byte[] Image(SKEncodedImageFormat format, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Blue);
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(format, 100);
        return encoded.ToArray();
    }

    [Theory, Trait("Requirement", "L2-040/AC3")]
    [InlineData("image/svg+xml", "venue.svg")]
    [InlineData("image/png", "venue.png")]
    public async Task Given_an_invalid_logo_when_uploaded_then_a_field_error_preserves_the_event(string mediaType, string name)
    {
        using var client = await factory.AdministratorBrowser();
        var original = await Create(client);
        using var response = await Upload(client, original, "<svg><script>invalid</script></svg>"u8.ToArray(), mediaType, name);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("logo", out _));
        var unchanged = await client.GetFromJsonAsync<JsonElement>($"/api/admin/events/{original.GetProperty("id").GetGuid()}");
        Assert.Equal(original.GetProperty("version").GetString(), unchanged.GetProperty("version").GetString());
    }

    private static async Task<JsonElement> Create(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.PostAsJsonAsync("/api/admin/events", new { title = "Venue branding" });
        response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<HttpResponseMessage> Upload(HttpClient client, JsonElement original, byte[] bytes, string mediaType,
        string name = "venue.png", string? operationId = null)
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        form.Add(file, "file", name);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{original.GetProperty("id").GetGuid()}/logo") { Content = form };
        request.Headers.Add("If-Match", $"\"{original.GetProperty("version").GetString()}\"");
        request.Headers.Add("Idempotency-Key", operationId ?? Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }
}
