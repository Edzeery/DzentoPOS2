using System.Text;

namespace DzentoPOS2.App.Diagnostics;

/// <summary>
/// Best-effort rolling file log for unhandled/important exceptions. Writes one
/// file per day under <c>%LocalAppData%\DzentoPOS2\logs</c>. Never throws: a
/// logging failure must not mask the original error.
/// </summary>
public static class ExceptionLogger
{
    private static readonly object Gate = new();

    public static string LogDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DzentoPOS2",
            "logs");

    /// <summary>
    /// Appends the exception (with full details) to today's log file and returns
    /// the file path, or <c>null</c> when logging was not possible.
    /// </summary>
    public static string? Write(Exception exception, string? context = null)
    {
        if (exception is null)
            return null;

        try
        {
            Directory.CreateDirectory(LogDirectory);

            var path = Path.Combine(
                LogDirectory,
                $"errors-{DateTime.Now:yyyyMMdd}.log");

            var builder = new StringBuilder();
            builder.AppendLine(new string('-', 80));
            builder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {context}".TrimEnd());
            builder.AppendLine(ExceptionFormatter.BuildDetails(exception));

            lock (Gate)
            {
                File.AppendAllText(path, builder.ToString());
            }

            return path;
        }
        catch
        {
            // Logging must never throw.
            return null;
        }
    }
}