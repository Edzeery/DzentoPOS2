using System.Runtime.CompilerServices;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Helpers;

public static class DataGridViewStyleHelper
{
    // Tracks grids that already own a hover handler so re-theming the grid
    // never stacks duplicate event subscriptions.
    private static readonly ConditionalWeakTable<DataGridView, object> HoverBound = new();

    private static Color AccentColor => AppColors.Primary;
    private static Color AccentSoft => AppColors.GridSelectionBackground;
    private static Color InkColor => AppColors.Ink;
    private static Color BorderColor => AppColors.Border;
    private static Color GridLineColor => AppColors.GridLine;
    private static Color AltRowColor => AppColors.GridRowAlt;

    public static void ApplyBaseStyle(DataGridView grid)
    {
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
        grid.BorderStyle = BorderStyle.None;
        grid.BackgroundColor = AppColors.Surface;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
    }

    public static void ApplyModernStyle(DataGridView grid, bool withHover = false)
    {
        ApplyBaseStyle(grid);

        grid.Font = new Font("Segoe UI", 9.5F);
        grid.RowTemplate.Height = 44;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = GridLineColor;
        grid.EnableHeadersVisualStyles = false;

        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppColors.GridHeaderBackground,
            ForeColor = AppColors.GridHeaderForeground,
            Font = new Font("Segoe UI Semibold", 10F),
            Padding = new Padding(10, 0, 10, 0),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            SelectionBackColor = AppColors.GridHeaderBackground,
            SelectionForeColor = AppColors.GridHeaderForeground
        };
        grid.ColumnHeadersHeight = 44;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppColors.Surface,
            ForeColor = InkColor,
            Padding = new Padding(10, 0, 10, 0),
            SelectionBackColor = AccentSoft,
            SelectionForeColor = InkColor
        };

        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AltRowColor
        };

        if (withHover)
            ApplyRowHover(grid);
    }

    public static void ApplyLightStyle(DataGridView grid)
    {
        ApplyModernStyle(grid, false);
    }

    private static void ApplyRowHover(DataGridView grid)
    {
        if (HoverBound.TryGetValue(grid, out _)) return;
        HoverBound.Add(grid, new object());

        grid.CellMouseEnter += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            if (grid.Rows[e.RowIndex].Selected) return;
            grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = AccentSoft;
        };
        grid.CellMouseLeave += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            if (grid.Rows[e.RowIndex].Selected) return;
            bool isEven = e.RowIndex % 2 == 0;
            grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = isEven ? AppColors.Surface : AltRowColor;
        };
    }
}
