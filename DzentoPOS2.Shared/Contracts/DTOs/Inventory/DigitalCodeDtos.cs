using DzentoPOS2.Core.Enums;

namespace DzentoPOS2.Shared.Contracts.DTOs.Inventory;

/// <summary>A digital code row for the management grid.</summary>
public class DigitalCodeDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Pin { get; set; }

    public Guid VariantId { get; set; }

    /// <summary>Product / variant label shown in the grid.</summary>
    public string VariantName { get; set; } = string.Empty;

    public DigitalCodeStatus Status { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public Guid? SalesInvoiceId { get; set; }

    /// <summary>Invoice number that delivered the code (null while available).</summary>
    public string? InvoiceNumber { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>A sellable variant of a digital-kind product, for the selector.</summary>
public class DigitalVariantOptionDto
{
    public Guid VariantId { get; set; }

    public Guid ProductId { get; set; }

    /// <summary>"Product - Variant" (product name alone when the variant is default).</summary>
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>Outcome of an import: how many codes were added and how many were skipped.</summary>
public class DigitalCodeImportResultDto
{
    public int Imported { get; set; }

    /// <summary>Duplicates already present for the same variant, or blank lines.</summary>
    public int Skipped { get; set; }
}
