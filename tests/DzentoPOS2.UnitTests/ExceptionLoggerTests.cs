using DzentoPOS2.App.Diagnostics;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class ExceptionLoggerTests
{
    [Fact]
    public void Write_appends_the_exception_to_a_daily_log_file()
    {
        var marker = $"marker-{Guid.NewGuid():N}";
        var path = ExceptionLogger.Write(new InvalidOperationException(marker), "unit-test");

        try
        {
            Assert.NotNull(path);
            Assert.True(File.Exists(path));

            var content = File.ReadAllText(path!);

            Assert.Contains(marker, content);
            Assert.Contains("Category:", content);
            Assert.Contains("unit-test", content);
        }
        finally
        {
            if (path is not null && File.Exists(path))
                File.Delete(path);
        }
    }
}
