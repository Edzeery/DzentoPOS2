using DzentoPOS2.App.Services.Barcodes;
using Xunit;

namespace DzentoPOS2.UnitTests;

public class CameraUrlValidatorTests
{
    [Theory]
    [InlineData("http://192.168.1.20:4747/video", true)]
    [InlineData("https://example.com/stream", true)]
    [InlineData("http://localhost:8080/cam", true)]
    [InlineData("http://10.0.0.1", true)]
    [InlineData("https://cam.example.com:443/path/to/stream", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("   ", false)]
    [InlineData("ftp://example.com/video", false)]
    [InlineData("not-a-url", false)]
    [InlineData("http://", false)]
    [InlineData("http:///path", false)]
    [InlineData("http://example.com:99999/video", false)]
    [InlineData("http://example.com:0/video", false)]
    [InlineData("http://example.com:-1/video", false)]
    public void IsValid_returns_expected(string? url, bool expected)
    {
        Assert.Equal(expected, CameraUrlValidator.IsValid(url));
    }

    [Fact]
    public void Normalize_trims_and_returns_null_for_invalid()
    {
        Assert.Equal("http://example.com/v", CameraUrlValidator.Normalize("  http://example.com/v  "));
        Assert.Null(CameraUrlValidator.Normalize("invalid"));
        Assert.Null(CameraUrlValidator.Normalize(null));
    }
}