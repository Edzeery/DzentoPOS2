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

        /// <summary>Local (store) date, only used when <see cref="Preset"/> is Custom.</summary>
        public DateTime? CustomFrom { get; set; }

        /// <summary>Local (store) date, only used when <see cref="Preset"/> is Custom.</summary>
        public DateTime? CustomTo { get; set; }

        /// <summary>Optional filter by the user who owns the records.</summary>
        public Guid? UserId { get; set; }
    }
}