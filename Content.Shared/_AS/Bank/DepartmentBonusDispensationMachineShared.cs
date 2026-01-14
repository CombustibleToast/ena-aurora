// Aurora Song - Department Bonus Dispensation Machine Shared
// UI key and messages for the department bonus machine

using Robust.Shared.Serialization;

namespace Content.Shared._AS.Bank;

[Serializable, NetSerializable]
public enum DepartmentBonusDispensationMachineUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class DepartmentBonusDispensationMachineBoundUserInterfaceState : BoundUserInterfaceState
{
    public string DepartmentName { get; }
    public float AllocationRate { get; } // Aurora Song - Renamed from TaxRate to AllocationRate
    public int StoredAmount { get; }
    public int MaxStoredAmount { get; }
    public bool Enabled { get; }
    public TimeSpan NextWithdrawal { get; }
    public int CurrentDepartmentBalance { get; } // Aurora Song - Added to show expected next withdrawal

    public DepartmentBonusDispensationMachineBoundUserInterfaceState(
        string departmentName,
        float allocationRate, // Aurora Song - Renamed from taxRate to allocationRate
        int storedAmount,
        int maxStoredAmount,
        bool enabled,
        TimeSpan nextWithdrawal,
        int currentDepartmentBalance = 0) // Aurora Song - Added parameter
    {
        DepartmentName = departmentName;
        AllocationRate = allocationRate; // Aurora Song - Renamed from TaxRate to AllocationRate
        StoredAmount = storedAmount;
        MaxStoredAmount = maxStoredAmount;
        Enabled = enabled;
        NextWithdrawal = nextWithdrawal;
        CurrentDepartmentBalance = currentDepartmentBalance;
    }
}

[Serializable, NetSerializable]
public sealed class DepartmentBonusDispensationMachineEjectMessage : BoundUserInterfaceMessage
{
}
