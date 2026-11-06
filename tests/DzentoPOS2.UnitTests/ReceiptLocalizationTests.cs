using Xunit;

namespace DzentoPOS2.UnitTests;

/// <summary>
/// Guards the printed receipt against untranslated (hardcoded English) labels:
/// every label must come from <c>AppStrings</c>.
/// </summary>
public sealed class ReceiptLocalizationTests
{
    private static string LocateSalesForm()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "DzentoPOS2", "Forms", "Sales", "SalesForm.cs");
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        throw new FileNotFoundException("SalesForm.cs was not found above the test output folder.");
    }

    [Fact]
    public void Receipt_builder_has_no_hardcoded_english_labels()
    {
        var source = File.ReadAllText(LocateSalesForm());

        var start = source.IndexOf("private void PrintLastInvoice()", StringComparison.Ordinal);
        Assert.True(start >= 0, "PrintLastInvoice not found.");

        var end = source.IndexOf("private static Label MakeLabel", start, StringComparison.Ordinal);
        Assert.True(end > start, "Could not isolate the receipt builder.");

        var region = source[start..end];

        // No literal English label followed by a value, e.g. Text($"Change: ...
        foreach (var label in new[] { "Invoice", "Date", "Subtotal", "Discount", "Tax", "Total", "Paid", "Remaining", "Change" })
            Assert.DoesNotContain($"Text($\"{label}", region);

        // The receipt must use the localized keys instead.
        Assert.Contains("AppStrings.Subtotal", region);
        Assert.Contains("AppStrings.Discount", region);
        Assert.Contains("AppStrings.TaxAmount", region);
        Assert.Contains("AppStrings.Total", region);
        Assert.Contains("AppStrings.PaidAmount", region);
        Assert.Contains("AppStrings.RemainingAmount", region);
        Assert.Contains("AppStrings.Change", region);
        Assert.Contains("AppStrings.ReceiptInvoice", region);
    }
}