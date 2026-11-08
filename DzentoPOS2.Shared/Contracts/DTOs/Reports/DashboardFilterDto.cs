namespace DzentoPOS2.Shared.Contracts.DTOs.Reports
{
    public enum DashboardDatePreset
    {
        Today = 0,
        Yesterday = 1,
        Last7Days = 2,
        ThisMonth = 3,
        Custom = 4
    }

    public class DashboardFilterDto
    {
        public DashboardDatePreset Preset { get; set; } = DashboardDatePreset.Today;

        public DateTime? CustomFrom { get; set; }

        public DateTime? CustomTo { get; set; }

        public Guid? UserId { get; set; }

        public bool ViewAllBranches { get; set; }
    }
}