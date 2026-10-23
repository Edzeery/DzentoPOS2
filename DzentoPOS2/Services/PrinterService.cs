using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace DzentoPOS2.WinForms.Services;

/// <summary>Description of the page that has to be printed.</summary>
/// <param name="PrinterName">Target printer (empty = system default).</param>
/// <param name="PaperSize">Name of the paper size (A4, Letter, ...).</param>
/// <param name="Landscape">Print in landscape orientation.</param>
/// <param name="ReceiptWidthMm">Receipt width in millimetres (58 / 80 for thermal printers).</param>
/// <param name="StoreName">Store name printed on the page header.</param>
/// <param name="CurrencyCode">Currency used by the printed amounts.</param>
/// <param name="Title">Title printed under the store name.</param>
/// <param name="Body">Free text body of the page.</param>
public sealed record PrintRequest(
    string PrinterName,
    string PaperSize,
    bool Landscape,
    int ReceiptWidthMm,
    string StoreName,
    string CurrencyCode,
    string Title,
    string Body);

/// <summary>
/// Thin wrapper around the Windows printing stack: lists the installed
/// printers, exposes the default printer and prints a diagnostic page so the
/// user can verify the printer settings from the settings screen.
/// </summary>
public static class PrinterService
{
    private const float MmToPx = 96f / 25.4f;

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SetDefaultPrinterW")]
    private static extern bool SetDefaultPrinterNative(string printerName);

    /// <summary>All printers installed on the machine.</summary>
    public static IReadOnlyList<string> GetInstalledPrinters()
    {
        var printers = new List<string>();

        try
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                if (!string.IsNullOrWhiteSpace(printer))
                    printers.Add(printer);
            }
        }
        catch
        {
            // The printing stack may be unavailable (no spooler service).
        }

        return printers;
    }

    /// <summary>Name of the system default printer (empty when none is set).</summary>
    public static string GetDefaultPrinter()
    {
        try
        {
            var settings = new PrinterSettings();
            return settings.PrinterName ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>Sets the system default printer for the current user.</summary>
    public static bool SetDefaultPrinter(string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName)) return false;

        try
        {
            return SetDefaultPrinterNative(printerName);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Paper sizes supported by a printer.</summary>
    public static IReadOnlyList<string> GetPaperSizes(string printerName)
    {
        var sizes = new List<string>();

        try
        {
            var settings = new PrinterSettings();
            if (!string.IsNullOrWhiteSpace(printerName))
                settings.PrinterName = printerName;

            foreach (PaperSize size in settings.PaperSizes)
            {
                if (!string.IsNullOrWhiteSpace(size.PaperName))
                    sizes.Add(size.PaperName);
            }
        }
        catch
        {
            // Ignore: the caller falls back to the generic sizes.
        }

        return sizes;
    }

    /// <summary>True when the printer exists on this machine.</summary>
    public static bool PrinterExists(string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName)) return false;
        return GetInstalledPrinters()
            .Any(p => string.Equals(p, printerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Prints a small diagnostic page. Returns false and a message when the
    /// page could not be sent to the printer.
    /// </summary>
    public static bool PrintTestPage(PrintRequest request, out string error)
    {
        error = string.Empty;

        try
        {
            using var document = new PrintDocument
            {
                DocumentName = "DzentoPOS2 - Test Page"
            };

            if (!string.IsNullOrWhiteSpace(request.PrinterName))
                document.PrinterSettings.PrinterName = request.PrinterName;

            ApplyOrientation(document.DefaultPageSettings, request.Landscape);
            ApplyPaperSize(document.DefaultPageSettings, request);

            document.PrintPage += (_, e) => DrawTestPage(e, request);

            document.Print();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static void ApplyOrientation(PageSettings settings, bool landscape)
    {
        try
        {
            PaperSize current = settings.PaperSize;
            if (current == null) return;

            bool currentlyLandscape = current.Width > current.Height;
            if (currentlyLandscape == landscape) return;

            settings.PaperSize = new PaperSize(
                current.PaperName,
                landscape ? current.Height : current.Width,
                landscape ? current.Width : current.Height);
        }
        catch
        {
            // Keep the printer default orientation when it cannot be changed.
        }
    }

    private static void ApplyPaperSize(PageSettings settings, PrintRequest request)
    {
        try
        {
            // Thermal receipt printers use a custom roll width instead of a named size.
            if (request.ReceiptWidthMm is 58 or 80)
            {
                float width = request.ReceiptWidthMm * MmToPx;
                float height = Math.Max(width * 4f, 200f * MmToPx);

                if (request.Landscape)
                    (width, height) = (height, width);

                settings.PaperSize = new PaperSize(
                    $"Receipt {request.ReceiptWidthMm}mm",
                    (int)width,
                    (int)height);
                return;
            }

            if (string.IsNullOrWhiteSpace(request.PaperSize)) return;

            foreach (PaperSize size in settings.PrinterSettings.PaperSizes)
            {
                if (!string.Equals(size.PaperName, request.PaperSize, StringComparison.OrdinalIgnoreCase))
                    continue;

                settings.PaperSize = size;
                return;
            }
        }
        catch
        {
            // The printer may not support the requested paper - the default is kept.
        }
    }

    private static void DrawTestPage(PrintPageEventArgs e, PrintRequest request)
    {
        var g = e.Graphics;
        if (g is null) return;

        var bounds = e.MarginBounds;

        using var titleFont = new Font("Segoe UI Semibold", 16F);
        using var bodyFont = new Font("Segoe UI", 10F);
        using var mutedFont = new Font("Segoe UI", 9F);
        using var linePen = new Pen(Color.Gray, 1f);
        using var textBrush = new SolidBrush(Color.Black);
        using var mutedBrush = new SolidBrush(Color.DimGray);

        var y = bounds.Top;

        g.DrawString(
            string.IsNullOrWhiteSpace(request.StoreName) ? "DzentoPOS2" : request.StoreName,
            titleFont,
            textBrush,
            bounds.Left,
            y);
        y += 34;

        g.DrawLine(linePen, bounds.Left, y, bounds.Right, y);
        y += 12;

        g.DrawString(request.Title, bodyFont, textBrush, bounds.Left, y);
        y += 24;

        g.DrawString(request.Body, bodyFont, textBrush, bounds.Left, y);
        y += 28;

        g.DrawString(
            $"Printer: {request.PrinterName} | Paper: {request.PaperSize} | " +
            $"Orientation: {(request.Landscape ? "Landscape" : "Portrait")} | " +
            $"Receipt: {request.ReceiptWidthMm}mm | Currency: {request.CurrencyCode}",
            mutedFont,
            mutedBrush,
            bounds.Left,
            y);
        y += 20;

        g.DrawString(
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            mutedFont,
            mutedBrush,
            bounds.Left,
            y);

        // A frame makes it obvious on the paper that the layout is correct.
        g.DrawRectangle(linePen, bounds);
    }
}
