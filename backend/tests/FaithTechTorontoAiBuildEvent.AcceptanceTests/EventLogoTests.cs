using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SkiaSharp;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventLogoTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-040/AC3")]
    [InlineData("image/jpeg", "venue.jpg", false)]
    [InlineData("image/png", "venue.exe", false)]
    [InlineData("image/png", "venue.png", true)]
    public async Task Given_mismatched_or_truncated_image_content_when_uploaded_then_no_logo_is_committed(string mediaType, string name, bool truncated)
    {
        using var client = await factory.AdministratorBrowser();
        var original = await Create(client);
        var bytes = Image(SKEncodedImageFormat.Png, 100, 100);
        if (truncated) bytes = bytes[..(bytes.Length / 2)];
        using var response = await Upload(client, original, bytes, mediaType, name);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.True((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors").TryGetProperty("logo", out _));
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/admin/events/{original.GetProperty("id").GetGuid()}/logo")).StatusCode);
    }

    [Fact, Trait("Requirement", "L2-044/AC4;L2-044/AC6;L2-045/AC3")]
    public async Task Given_concurrent_logo_uploads_when_retried_then_one_asset_and_audit_are_committed_and_changed_payload_is_rejected()
    {
        using var client = await factory.AdministratorBrowser();
        var original = await Create(client);
        var bytes = Image(SKEncodedImageFormat.Png, 3, 2);
        var key = Guid.NewGuid().ToString();
        var responses = await Task.WhenAll(Upload(client, original, bytes, "image/png", operationId: key),
            Upload(client, original, bytes, "image/png", operationId: key));
        foreach (var response in responses) response.EnsureSuccessStatusCode();
        Assert.Equal(await responses[0].Content.ReadAsStringAsync(), await responses[1].Content.ReadAsStringAsync());
        using var stale = await Upload(client, original, bytes, "image/png");
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("stale-version", (await stale.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        using var changed = await Upload(client, original, Image(SKEncodedImageFormat.Png, 4, 2), "image/png", operationId: key);
        Assert.Equal(HttpStatusCode.Conflict, changed.StatusCode);
        Assert.Equal("operation-key-reused", (await changed.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        var id = original.GetProperty("id").GetGuid();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        var audit = await db.AuditRecords.Where(x => x.EventId == id && x.Action == "event-logo-saved").SingleAsync();
        Assert.NotEqual(Guid.Empty, audit.ActorId);
        Assert.Equal("succeeded", audit.Outcome);
        Assert.Single(await db.OperationReceipts.Where(x => x.EventId == id).ToListAsync());
        var stored = await db.Events.Where(x => x.Id == id).SingleAsync();
        Assert.Equal(3, (await db.Logos.SingleAsync(x => x.Id == stored.LogoId)).Width);
        foreach (var response in responses) response.Dispose();
    }

    [Fact, Trait("Requirement", "L2-038;L2-041;L2-044/AC4")]
    public async Task Given_a_logo_upload_when_authorization_or_antiforgery_is_missing_then_no_image_is_saved()
    {
        using var client = await factory.AdministratorBrowser();
        var original = await Create(client);
        var bytes = Image(SKEncodedImageFormat.Png, 3, 2);
        using var anonymous = factory.Browser();
        using var denied = await Upload(anonymous, original, bytes, "image/png");
        Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        using var noCsrf = await Upload(client, original, bytes, "image/png");
        Assert.Equal(HttpStatusCode.BadRequest, noCsrf.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/admin/events/{original.GetProperty("id").GetGuid()}/logo")).StatusCode);
    }

    [Theory, Trait("Requirement", "L2-040/AC3")]
    [InlineData(1, 1, 2097152, true)]
    [InlineData(1, 1, 2097153, false)]
    [InlineData(4096, 1, 0, true)]
    [InlineData(4097, 1, 0, false)]
    [InlineData(1, 4096, 0, true)]
    [InlineData(1, 4097, 0, false)]
    public async Task Given_logo_limits_when_uploaded_then_exact_boundaries_are_accepted_and_excess_preserves_the_event(int width, int height, int size, bool accepted)
    {
        using var client = await factory.AdministratorBrowser();
        var original = await Create(client);
        var bytes = Image(SKEncodedImageFormat.Png, width, height);
        if (size > 0) Array.Resize(ref bytes, size);
        using var response = await Upload(client, original, bytes, "image/png");
        Assert.Equal(accepted ? HttpStatusCode.OK : HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        if (accepted) { Assert.Equal(width, result.GetProperty("logo").GetProperty("width").GetInt32()); }
        else {
            Assert.True(result.GetProperty("errors").TryGetProperty("logo", out _));
            var current = await client.GetFromJsonAsync<JsonElement>($"/api/admin/events/{original.GetProperty("id").GetGuid()}");
            Assert.Equal(original.GetProperty("version").GetString(), current.GetProperty("version").GetString());
            Assert.Equal(JsonValueKind.Null, current.GetProperty("logo").ValueKind);
        }
    }

    [Theory, Trait("Requirement", "L2-040/AC3")]
    [InlineData(1, 40, 20, 0xffff0000u)]
    [InlineData(2, 40, 20, 0xff0000ffu)]
    [InlineData(3, 40, 20, 0xffffff00u)]
    [InlineData(4, 40, 20, 0xff00ff00u)]
    [InlineData(5, 20, 40, 0xffff0000u)]
    [InlineData(6, 20, 40, 0xff00ff00u)]
    [InlineData(7, 20, 40, 0xffffff00u)]
    [InlineData(8, 20, 40, 0xff0000ffu)]
    public async Task Given_an_oriented_jpeg_when_uploaded_then_the_served_image_preserves_its_visual_orientation(byte orientation, int width, int height, uint expectedColor)
    {
        using var client = await factory.AdministratorBrowser();
        var original = await Create(client);
        using var bitmap = new SKBitmap(40, 20);
        bitmap.Erase(SKColors.Blue);
        using (var canvas = new SKCanvas(bitmap)) {
            using var paint = new SKPaint { Color = SKColors.Red };
            canvas.DrawRect(0, 0, 20, 20, paint);
            paint.Color = SKColors.Lime; canvas.DrawRect(0, 10, 20, 10, paint);
            paint.Color = SKColors.Yellow; canvas.DrawRect(20, 10, 20, 10, paint);
        }
        using var image = SKImage.FromBitmap(bitmap);
        using var jpeg = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        byte[] exif = [0xff, 0xe1, 0, 34, 69, 120, 105, 102, 0, 0, 73, 73, 42, 0, 8, 0, 0, 0,
            1, 0, 18, 1, 3, 0, 1, 0, 0, 0, orientation, 0, 0, 0, 0, 0, 0, 0];
        var raw = jpeg.ToArray();
        byte[] bytes = [..raw[..2], ..exif, ..raw[2..]];
        using var response = await Upload(client, original, bytes, "image/jpeg", "rotated.jpg");
        response.EnsureSuccessStatusCode();
        var saved = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(width, saved.GetProperty("logo").GetProperty("width").GetInt32());
        Assert.Equal(height, saved.GetProperty("logo").GetProperty("height").GetInt32());
        using var decoded = SKBitmap.Decode(await client.GetByteArrayAsync($"/api/admin/events/{original.GetProperty("id").GetGuid()}/logo"));
        var corner = decoded.GetPixel(3, 3);
        var expected = new SKColor(expectedColor);
        Assert.InRange(Math.Abs(corner.Red - expected.Red), 0, 10);
        Assert.InRange(Math.Abs(corner.Green - expected.Green), 0, 10);
        Assert.InRange(Math.Abs(corner.Blue - expected.Blue), 0, 10);
    }

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
