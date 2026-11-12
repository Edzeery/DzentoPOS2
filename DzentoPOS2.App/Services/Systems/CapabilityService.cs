using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Systems;

public interface ICapabilityService
{
    Task<bool> HasCapabilityAsync(BranchCapability capability, CancellationToken ct = default);
    Task<BranchCapability> GetCapabilitiesAsync(CancellationToken ct = default);
    Task SetCapabilitiesAsync(Guid branchId, BranchCapability capabilities, CancellationToken ct = default);
}

public class CapabilityService : ICapabilityService
{
    private readonly AppDbContext _context;
    private readonly IActiveBranchContext _activeBranch;

    public CapabilityService(AppDbContext context, IActiveBranchContext activeBranch)
    {
        _context = context;
        _activeBranch = activeBranch;
    }

    public async Task<bool> HasCapabilityAsync(BranchCapability capability, CancellationToken ct = default)
    {
        var caps = await GetCapabilitiesAsync(ct);
        return (caps & capability) == capability;
    }

    public async Task<BranchCapability> GetCapabilitiesAsync(CancellationToken ct = default)
    {
        var branchId = _activeBranch.ActiveBranchId;
        if (branchId == Guid.Empty)
            return BranchCapability.None;

        var activities = await _context.BranchActivities
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && a.IsEnabled)
            .ToListAsync(ct);

        var caps = BranchCapability.None;
        foreach (var activity in activities)
        {
            caps |= GetCapabilitiesForActivity(activity.ActivityType);
        }

        return caps;
    }

    public async Task SetCapabilitiesAsync(Guid branchId, BranchCapability capabilities, CancellationToken ct = default)
    {
        if (branchId == Guid.Empty)
            return; // No active branch; nothing to do.

        // Remove existing activities for this branch.
        var existing = await _context.BranchActivities
            .Where(a => a.BranchId == branchId)
            .ToListAsync(ct);
        _context.BranchActivities.RemoveRange(existing);

        // Add new activities based on capabilities.
        var activityTypes = GetActivityTypesForCapabilities(capabilities);
        foreach (var type in activityTypes)
        {
            _context.BranchActivities.Add(new Core.Entities.Businesses.BranchActivity
            {
                BranchId = branchId,
                ActivityType = type,
                IsEnabled = true
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    private static BranchCapability GetCapabilitiesForActivity(ActivityType type) => type switch
    {
        ActivityType.Grocery => BranchCapability.ExpiryDate | BranchCapability.WeightUnit | BranchCapability.BatchTracking,
        ActivityType.Produce => BranchCapability.WeightUnit | BranchCapability.BatchTracking | BranchCapability.ExpiryDate,
        ActivityType.Phones => BranchCapability.SerialNumber | BranchCapability.IMEI | BranchCapability.Warranty | BranchCapability.RepairTicket,
        ActivityType.Appliances => BranchCapability.SerialNumber | BranchCapability.Warranty | BranchCapability.RepairTicket,
        ActivityType.IT => BranchCapability.SerialNumber | BranchCapability.License | BranchCapability.DigitalCode,
        ActivityType.Repair => BranchCapability.RepairTicket | BranchCapability.SerialNumber | BranchCapability.Warranty,
        ActivityType.Digital => BranchCapability.DigitalCode | BranchCapability.License | BranchCapability.NoStock,
        ActivityType.MultiService => BranchCapability.SerialNumber | BranchCapability.IMEI | BranchCapability.Warranty |
            BranchCapability.RepairTicket | BranchCapability.ExpiryDate | BranchCapability.WeightUnit |
            BranchCapability.BatchTracking | BranchCapability.DigitalCode | BranchCapability.License | BranchCapability.NoStock,
        _ => BranchCapability.None
    };

    private static List<ActivityType> GetActivityTypesForCapabilities(BranchCapability caps)
    {
        var types = new List<ActivityType>();
        if ((caps & BranchCapability.WeightUnit) != 0) types.Add(ActivityType.Grocery);
        if ((caps & BranchCapability.SerialNumber) != 0) types.Add(ActivityType.Phones);
        if ((caps & BranchCapability.DigitalCode) != 0) types.Add(ActivityType.Digital);
        if ((caps & BranchCapability.RepairTicket) != 0) types.Add(ActivityType.Repair);
        return types.Count > 0 ? types : new List<ActivityType> { ActivityType.MultiService };
    }
}
