using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Systems;

public class DocumentSequence : AuditableEntity
{
    public string Prefix { get; set; } = string.Empty;

    public string DateKey { get; set; } = string.Empty;

    public long LastValue { get; set; }
}
