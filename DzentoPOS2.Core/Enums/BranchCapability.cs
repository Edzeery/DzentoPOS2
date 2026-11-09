using System;

namespace DzentoPOS2.Core.Enums
{
    /// <summary>
    /// Atomic capabilities that can be enabled per branch.
    /// </summary>
    [Flags]
    public enum BranchCapability
    {
        None = 0,
        SerialNumber = 1,
        IMEI = 2,
        Warranty = 4,
        RepairTicket = 8,
        ExpiryDate = 16,
        WeightUnit = 32,
        BatchTracking = 64,
        DigitalCode = 128,
        License = 256,
        NoStock = 512,
    }
}
