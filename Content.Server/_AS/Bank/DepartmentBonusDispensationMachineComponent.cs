// Aurora Song - Department Bonus Dispensation Machine
// Periodically allocates a percentage from a department's bank account as staff bonuses

using Content.Shared._NF.Bank.Components;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._AS.Bank;

/// <summary>
/// Periodically allocates a percentage of a department's budget as staff bonuses and stores it as currency inside.
/// Can be dispensed by someone with the appropriate department access.
/// </summary>
[RegisterComponent]
public sealed partial class DepartmentBonusDispensationMachineComponent : Component
{
    /// <summary>
    /// The department bank account to withdraw from
    /// </summary>
    [DataField(required: true)]
    public SectorBankAccount TargetDepartment;

    /// <summary>
    /// Percentage of the department's balance to allocate for bonuses (0.0 to 1.0)
    /// Default: 0.1 (10%)
    /// </summary>
    [DataField]
    public float AllocationRate = 0.1f; // Aurora Song - Renamed from TaxRate to AllocationRate

    /// <summary>
    /// How often to allocate bonus funds, in seconds
    /// Default: 300 (5 minutes)
    /// </summary>
    [DataField]
    public float WithdrawalInterval = 300f;

    /// <summary>
    /// Next time to attempt bonus allocation
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextWithdrawal = TimeSpan.Zero;

    /// <summary>
    /// Total amount of currency currently stored in the machine
    /// </summary>
    [DataField]
    public int StoredAmount = 0;

    /// <summary>
    /// Maximum amount that can be stored in the machine
    /// If reached, bonus allocations will be paused until dispensed
    /// </summary>
    [DataField]
    public int MaxStoredAmount = 100000;

    /// <summary>
    /// Sound to play when allocating bonus funds
    /// </summary>
    [DataField]
    public SoundSpecifier? WithdrawSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    /// <summary>
    /// Sound to play when dispensing currency
    /// </summary>
    [DataField]
    public SoundSpecifier? EjectSound = new SoundPathSpecifier("/Audio/Machines/machine_vend.ogg");

    /// <summary>
    /// Currency prototype to spawn when dispensing
    /// </summary>
    [DataField]
    public string CurrencyPrototype = "SpaceCash1000";

    /// <summary>
    /// Whether the machine is currently enabled
    /// </summary>
    [DataField]
    public bool Enabled = true;

    /// <summary>
    /// Accumulator for UI update timing (not serialized)
    /// </summary>
    public float UiUpdateAccumulator = 0f;
}
