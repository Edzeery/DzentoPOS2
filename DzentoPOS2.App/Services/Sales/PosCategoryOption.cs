using System;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>A category that can be offered as a filter chip on the POS screen.</summary>
public readonly record struct PosCategoryOption(Guid Id, string Name);
