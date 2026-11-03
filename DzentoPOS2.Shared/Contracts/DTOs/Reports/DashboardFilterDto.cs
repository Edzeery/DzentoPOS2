namespace DzentoPOS2.Shared.Contracts.DTOs.Reports
{
    public enum DashboardDatePreset
    {
        Today = 0,
        Yesterday = 1,
        LastWeek = 2,
        Month = 3,
        Custom = 4
    }

    public class DashboardFilterDto
    {
        public DashboardDatePreset DatePreset { get; set; } = DashboardDatePreset.Month;
        public DateTime? CustomFromUtc { get; set; }
        public DateTime? CustomToUtc { get; set; }
        public Guid? CustomerId { get; set; }
    }
}