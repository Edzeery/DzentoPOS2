using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class PosImagePathTests : IDisposable
{
    private readonly string _root;

    public PosImagePathTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "DzentoPOS2.ImageTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_root, "products"));
    }

    [Fact]
    public void Resolve_RelativePathUnderImagesRoot()
    {
        var file = CreateFile("products/a.jpg");

        var resolved = PosImagePath.Resolve(_root, "products/a.jpg");

        Assert.Equal(Path.GetFullPath(file), resolved);
    }

    [Fact]
    public void Resolve_ToleratesLeadingImagesSegment()
    {
        var file = CreateFile("products/b.jpg");

        var resolved = PosImagePath.Resolve(_root, "images/products/b.jpg");

        Assert.Equal(Path.GetFullPath(file), resolved);
    }

    [Fact]
    public void Resolve_AcceptsAbsolutePath()
    {
        var file = CreateFile("products/c.jpg");

        var resolved = PosImagePath.Resolve(_root, file);

        Assert.Equal(Path.GetFullPath(file), resolved);
    }

    [Fact]
    public void Resolve_ReturnsNullForMissingFile()
    {
        Assert.Null(PosImagePath.Resolve(_root, "products/missing.jpg"));
    }

    [Fact]
    public void Resolve_RejectsPathTraversal()
    {
        Assert.Null(PosImagePath.Resolve(_root, "../outside.jpg"));
        Assert.Null(PosImagePath.Resolve(_root, "products/../../outside.jpg"));
    }

    [Fact]
    public void Resolve_ReturnsNullForEmptyInput()
    {
        Assert.Null(PosImagePath.Resolve(_root, null));
        Assert.Null(PosImagePath.Resolve(_root, "  "));
    }

    private string CreateFile(string relative)
    {
        var full = Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar));
        File.WriteAllBytes(full, new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 });
        return full;
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
