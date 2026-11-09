using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Systems;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed;

/// <summary>
/// One-time seeder: migrates old AppSettings module flags to BranchActivity rows.
/// Runs once per database (tracked by 'modules.migrated' flag).
/// </summary>
public static class BranchActivitySeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Check if already migrated
        var migrated = await context.AppSettings
            .AnyAsync(s => s.Key == "modules.migrated" && s.Value == "true");

        if (migrated) return;

        // Read old module flags
        var settings = await context.AppSettings
            .Where(s => s.Key.StartsWith("modules."))
            .ToListAsync();

        var groceryEnabled = settings.FirstOrDefault(s => s.Key == "modules.grocery.enabled")?.Value == "true";
        var phonesEnabled = settings.FirstOrDefault(s => s.Key == "modules.phones.enabled")?.Value == "true";
        var digitalEnabled = settings.FirstOrDefault(s => s.Key == "modules.digital.enabled")?.Value == "true";
        var generalEnabled = settings.FirstOrDefault(s => s.Key == "modules.general.enabled")?.Value == "true";

        var branches = await context.Branches.Where(b => b.IsActive).ToListAsync();

        foreach (var branch in branches)
        {
            var capabilities = BranchCapability.None;

            if (groceryEnabled) capabilities |= BranchCapability.ExpiryDate | BranchCapability.WeightUnit | BranchCapability.BatchTracking;
            if (phonesEnabled) capabilities |= BranchCapability.SerialNumber | BranchCapability.IMEI | BranchCapability.Warranty | BranchCapability.RepairTicket;
            if (digitalEnabled) capabilities |= BranchCapability.DigitalCode | BranchCapability.License | BranchCapability.NoStock;
            if (generalEnabled) capabilities |= BranchCapability.SerialNumber | BranchCapability.IMEI | BranchCapability.Warranty |
                BranchCapability.RepairTicket | BranchCapability.ExpiryDate | BranchCapability.WeightUnit |
                BranchCapability.BatchTracking | BranchCapability.DigitalCode | BranchCapability.License | BranchCapability.NoStock;

            // If no modules enabled, default to MultiService
            if (capabilities == BranchCapability.None)
                capabilities = BranchCapability.SerialNumber | BranchCapability.IMEI | BranchCapability.Warranty |
                    BranchCapability.RepairTicket | BranchCapability.ExpiryDate | BranchCapability.WeightUnit |
                    BranchCapability.BatchTracking | BranchCapability.DigitalCode | BranchCapability.License | BranchCapability.NoStock;

            // Map capabilities to activity types
            var activityTypes = GetActivityTypes(capabilities);

            foreach (var type in activityTypes)
            {
                var exists = await context.BranchActivities
                    .AnyAsync(a => a.BranchId == branch.Id && a.ActivityType == type);
                if (!exists)
                {
                    context.BranchActivities.Add(new Core.Entities.Businesses.BranchActivity
                    {
                        BranchId = branch.Id,
                        ActivityType = type,
                        IsEnabled = true
                    });
                }
            }
        }

        await context.SaveChangesAsync();

        // Mark as migrated
        context.AppSettings.Add(new Core.Entities.Systems.AppSetting
        {
            Key = "modules.migrated",
            Value = "true",
            Group = "Modules"
        });
        await context.SaveChangesAsync();
    }

    private static List<ActivityType> GetActivityTypes(BranchCapability caps)
    {
        var types = new List<ActivityType>();
        if ((caps & BranchCapability.WeightUnit) != 0) types.Add(ActivityType.Grocery);
        if ((caps & BranchCapability.SerialNumber) != 0) types.Add(ActivityType.Phones);
        if ((caps & BranchCapability.DigitalCode) != 0) types.Add(ActivityType.Digital);
        if ((caps & BranchCapability.RepairTicket) != 0 && !types.Contains(ActivityType.Phones)) types.Add(ActivityType.Repair);
        return types.Count > 0 ? types : new List<ActivityType> { ActivityType.MultiService };
    }
}
