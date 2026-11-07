namespace DzentoPOS2.Shared.Contracts.DTOs.Inventory
{
    public sealed class InventoryValuationDto
    {
        public decimal TotalQuantity { get; set; }
        public decimal CostValue { get; set; }
        public decimal RetailValue { get; set; }
        public decimal PotentialProfit { get; set; }
        public decimal PotentialMarginPct { get; set; }
        public decimal ExpectedProfit { get; set; }   // realistic NRV
        public decimal AtRiskProfit { get; set; }      // sum of negative units
        public int WarningNoSalePrice { get; set; }
        public int WarningNegativeMargin { get; set; }
        public int WarningZeroCost { get; set; }
        public decimal DiscountFactorAvg { get; set; }
        public decimal FeeFactor { get; set; }
    }
}