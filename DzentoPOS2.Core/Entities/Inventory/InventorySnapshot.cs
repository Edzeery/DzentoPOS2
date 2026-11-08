using System;
using DzentoPOS2.Core.Common;

namespace DzentoPOS2.Core.Entities.Inventory
{
    public class InventorySnapshot : AuditableEntity
    {
        /// <summary>Local (store) date of the snapshot; one row per day.</summary>
        public DateTime SnapshotDate { get; set; }

        public decimal TotalQuantity { get; set; }
        public decimal CostValue { get; set; }
        public decimal RetailValue { get; set; }
        public decimal ExpectedProfit { get; set; }
        public decimal AtRiskProfit { get; set; }
        public decimal DiscountFactorAvg { get; set; }
        public decimal FeeFactor { get; set; }
    }
}