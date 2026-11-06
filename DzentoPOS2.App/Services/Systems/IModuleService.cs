using System;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Services.Systems;

public enum ActivityModule
{
    Grocery,
    Phones,
    Digital,
    GeneralRetail
}

public interface IModuleService
{
    Task<bool> IsEnabledAsync(ActivityModule module);
    Task SetEnabledAsync(ActivityModule module, bool enabled);
    Task<bool> IsConfiguredAsync();
    Task SetConfiguredAsync(bool configured);
    Task<Dictionary<ActivityModule, bool>> GetAllAsync();
}