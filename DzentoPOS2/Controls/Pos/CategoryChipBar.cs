using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls.Pos;

/// <summary>
/// Horizontal, scrollable strip of category chips. The first chip is always
/// "All"; the rest come from <see cref="SetCategories"/>. The control owns no
/// data access, so it is safe on a design surface.
/// </summary>
public partial class CategoryChipBar : UserControl, IThemeAware
{
    private readonly List<CategoryChip> _chips = new();

    /// <summary>Raised when the user picks a chip. Null means "All".</summary>
    public event EventHandler<PosCategoryFilterChangedEventArgs>? CategorySelected;

    public CategoryChipBar()
    {
        InitializeComponent();
        Font = AppStyle.BodyFont;

        // FlowLayoutPanel already mirrors its flow when the container is RTL,
        // and the form's localization helper additionally forces
        // FlowDirection.RightToLeft. Applying both reverses the chips twice,
        // so the panel is always kept in its natural left-to-right direction:
        // Windows mirrors it when RightToLeft is Yes.
        _flow.Layout += (_, _) =>
        {
            if (_flow.FlowDirection != FlowDirection.LeftToRight)
                _flow.FlowDirection = FlowDirection.LeftToRight;
        };

        if (DesignTime.IsRuntime(this))
            ApplyTheme();
    }

    /// <summary>Currently selected category; null is the "All" chip.</summary>
    public Guid? SelectedCategoryId { get; private set; }

    /// <summary>
    /// Rebuilds the chip list. Duplicate ids and blank names are ignored, and
    /// the previous selection is kept when that category still exists.
    /// </summary>
    public void SetCategories(IEnumerable<PosCategoryOption> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        if (IsDisposed) return;

        _flow.SuspendLayout();
        try
        {
            foreach (CategoryChip chip in _chips)
            {
                _flow.Controls.Remove(chip);
                chip.Dispose();
            }

            _chips.Clear();

            AddChip(null, AppStrings.All);

            var seen = new HashSet<Guid>();
            foreach (PosCategoryOption option in categories)
            {
                if (option.Id == Guid.Empty || string.IsNullOrWhiteSpace(option.Name))
                    continue;

                if (!seen.Add(option.Id))
                    continue;

                AddChip(option.Id, option.Name.Trim());
            }
        }
        finally
        {
            _flow.ResumeLayout();
        }

        Select(SelectedCategoryId, raiseEvent: true);
    }

    /// <summary>Selects a category without raising <see cref="CategorySelected"/>.</summary>
    public void Select(Guid? categoryId)
    {
        Select(categoryId, raiseEvent: false);
    }

    public void ApplyTheme()
    {
        if (IsDisposed) return;

        BackColor = Color.Transparent;
        _flow.BackColor = Color.Transparent;

        foreach (CategoryChip chip in _chips)
            chip.ApplyTheme();

        Invalidate();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Never touch runtime services or the theme from the designer.
        if (DesignTime.IsActive(this))
            return;

        ThemeManager.ThemeChanged += OnThemeChanged;
        ApplyTheme();
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        if (IsDisposed) return;

        if (InvokeRequired)
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(ApplyTheme));
            return;
        }

        ApplyTheme();
    }

    private void AddChip(Guid? categoryId, string text)
    {
        var chip = new CategoryChip
        {
            CategoryId = categoryId,
            Text = text,
            Margin = new Padding(0, 0, 8, 0),
            Height = 32
        };

        chip.Selected += (_, _) => Select(chip.CategoryId, raiseEvent: true);
        chip.Width = chip.GetPreferredSize(Size.Empty).Width;

        _chips.Add(chip);
        _flow.Controls.Add(chip);
    }

    private void Select(Guid? categoryId, bool raiseEvent)
    {
        bool found = categoryId is null;
        foreach (CategoryChip chip in _chips)
        {
            if (chip.CategoryId == categoryId)
                found = true;
        }

        if (!found)
            categoryId = null;

        foreach (CategoryChip chip in _chips)
            chip.IsSelected = chip.CategoryId == categoryId;

        bool changed = SelectedCategoryId != categoryId;
        SelectedCategoryId = categoryId;

        if (raiseEvent && changed)
            CategorySelected?.Invoke(this, new PosCategoryFilterChangedEventArgs(categoryId));
    }
}

/// <summary>Selected category on the POS chip bar; null is "All".</summary>
public sealed class PosCategoryFilterChangedEventArgs : EventArgs
{
    public PosCategoryFilterChangedEventArgs(Guid? categoryId)
    {
        CategoryId = categoryId;
    }

    public Guid? CategoryId { get; }
}
