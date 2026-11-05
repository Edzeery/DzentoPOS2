using DzentoPOS2.Core.Entities.Systems;
using DzentoPOS2.Core.Systems;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Systems;

public class ModuleService : IModuleService
{
    private readonly AppDbContext _context;

    public ModuleService(AppDbContext context)
    {
        _context = context;
    }

    private static string KeyFor(ActivityModule m) => m switch
    {
        ActivityModule.Grocery => AppSettingKeys.ModuleGrocery,
        ActivityModule.Phones => AppSettingKeys.ModulePhones,
        ActivityModule.Digital => AppSettingKeys.ModuleDigital,
        ActivityModule.GeneralRetail => AppSettingKeys.ModuleGeneral,
        _ => throw new ArgumentOutOfRangeException(nameof(m))
    };

    public async Task<bool> IsEnabledAsync(ActivityModule module)
    {
        var key = KeyFor(module);
        var val = await _context.AppSettings
            .AsNoTracking()
            .Where(x => x.Key == key)
            .Select(x => x.Value)
            .FirstOrDefaultAsync();
        return bool.TryParse(val, out var b) && b;
    }

    public async Task SetEnabledAsync(ActivityModule module, bool enabled)
    {
        var key = KeyFor(module);
        var setting = await _context.AppSettings.FirstOrDefaultAsync(x => x.Key == key);
        if (setting is null)
        {
            setting = new AppSetting { Key = key, Value = enabled.ToString().ToLowerInvariant(), Group = "Modules" };
            await _context.AppSettings.AddAsync(setting);
        }
        else
        {
            setting.Value = enabled.ToString().ToLowerInvariant();
        }
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsConfiguredAsync()
    {
        var val = await _context.AppSettings
            .AsNoTracking()
            .Where(x => x.Key == AppSettingKeys.ModulesConfigured)
            .Select(x => x.Value)
            .FirstOrDefaultAsync();
        return bool.TryParse(val, out var b) && b;
    }

    public async Task SetConfiguredAsync(bool configured)
    {
        var setting = await _context.AppSettings.FirstOrDefaultAsync(x => x.Key == AppSettingKeys.ModulesConfigured);
        if (setting is null)
        {
            setting = new AppSetting { Key = AppSettingKeys.ModulesConfigured, Value = configured.ToString().ToLowerInvariant(), Group = "Modules" };
            await _context.AppSettings.AddAsync(setting);
        }
        else
        {
            setting.Value = configured.ToString().ToLowerInvariant();
        }
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<ActivityModule, bool>> GetAllAsync()
    {
        var result = new Dictionary<ActivityModule, bool>();
        foreach (ActivityModule m in Enum.GetValues(typeof(ActivityModule)))
        {
            result[m] = await IsEnabledAsync(m);
        }
        return result;
    }
}