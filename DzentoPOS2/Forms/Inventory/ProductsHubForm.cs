using DzentoPOS2.App.Authorization;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.WinForms.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public partial class ProductsHubForm : LocalizableFormBase
{
    private readonly IServiceProvider _services;
    private readonly IAuthorizationGuard _guard;
    private readonly TabControl _tabs;

    public ProductsHubForm(IServiceProvider services, IAuthorizationGuard guard, ILanguageService languageService)
        : base(languageService)
    {
        _services = services;
        _guard = guard;
        InitializeComponent();
        _tabs = new TabControl { Dock = DockStyle.Fill };
        Controls.Add(_tabs);
        _tabs.SelectedIndexChanged += (_, _) => EnsureHosted(_tabs.SelectedTab);
    }

    protected override void LoadText()
    {
        Text = AppStrings.Products;
        if (_tabs.TabPages.Count > 0)
        {
            // Update titles for known tabs by index order: Products, (Categories), (Units)
            // We keep it simple: titles set at creation; refresh all by tag type name.
            foreach (TabPage page in _tabs.TabPages)
            {
                if ((page.Tag as Type) == typeof(ProductsForm)) page.Text = AppStrings.Products;
                else if ((page.Tag as Type) == typeof(ProductCategoriesForm)) page.Text = AppStrings.Categories;
                else if ((page.Tag as Type) == typeof(ProductUnitsForm)) page.Text = AppStrings.Units;
                else if ((page.Tag as Type) == typeof(DigitalCodesForm)) page.Text = AppStrings.DigitalCodesTitle;
            }
        }
    }

    protected override void OnInitialized()
    {
        RunSafe(async () =>
        {
            AddTab(AppStrings.Products, typeof(ProductsForm));
            if (await _guard.IsAllowedAsync(Permissions.CategoriesView))
                AddTab(AppStrings.Categories, typeof(ProductCategoriesForm));
            if (await _guard.IsAllowedAsync(Permissions.UnitsView))
                AddTab(AppStrings.Units, typeof(ProductUnitsForm));
            if (await _guard.IsAllowedAsync(Permissions.DigitalCodesView))
                AddTab(AppStrings.DigitalCodesTitle, typeof(DigitalCodesForm));

            if (_tabs.TabPages.Count > 0)
            {
                _tabs.SelectedIndex = 0;
                EnsureHosted(_tabs.SelectedTab);
            }
        });
    }

    protected override void OnLanguageChangedAfterLoadText()
    {
        // Forms hosted inside tabs handle their own localization via the
        // global LanguageChanged event; tab titles already refreshed in LoadText.
    }

    private void AddTab(string title, Type formType)
    {
        var page = new TabPage(title) { Tag = formType };
        _tabs.TabPages.Add(page);
    }

    private void EnsureHosted(TabPage? page)
    {
        if (page == null || page.Controls.Count > 0) return;
        var formType = page.Tag as Type;
        if (formType == null) return;
        var form = (Form)_services.GetRequiredService(formType);
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        page.Controls.Add(form);
        form.Show();
    }
}