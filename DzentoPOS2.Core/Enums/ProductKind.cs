namespace DzentoPOS2.Core.Enums;

/// <summary>
/// Kind of a product. Drives module-specific behaviour (weighed, serialized, service, digital).
/// </summary>
public enum ProductKind
{
    Standard = 0,
    Weighed = 1,
    Serialized = 2,
    Service = 3,
    DigitalCode = 4
}