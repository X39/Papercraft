using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Services.ResourceResolver;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class TroubleshootingImageTests
{
    [Fact]
    public async Task DefaultResolverRejectsUrlSources()
    {
        var resolver = new DefaultResourceResolver();

        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await resolver.ResolveImageAsync("https://example.com/logo.png", null));

        Assert.Contains("Only base64 encoded images", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DefaultResolverRejectsDataUriWithoutPayload()
    {
        var resolver = new DefaultResourceResolver();

        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await resolver.ResolveImageAsync("data:image/png;base64", null));

        Assert.Contains("Expected data:image/...;base64,...", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidBase64ThatIsNotAnImageFailsDuringInitialization()
    {
        using var control = new ImageControl(new DefaultResourceResolver())
        {
            Source = Convert.ToBase64String([1, 2, 3, 4]),
        };

        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await control.InitializeControlAsync(null));

        Assert.Contains("Unsupported image format", exception.Message, StringComparison.Ordinal);
    }
}
