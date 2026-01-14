// Aurora Song - AS Medical Bounty System
// Based on New Frontier Station 14's Medical Bounty System
// Original implementation: https://github.com/new-frontiers-14/frontier-station-14
using Content.Shared._NF.Bank.Components;

namespace Content.Server._AS.Medical;

/// <summary>
/// Aurora Song: Component for entities that can receive AS medical bounty payments
/// </summary>
[RegisterComponent]
public sealed partial class ASMedicalBountyBankPaymentComponent : Component
{
    [DataField(required: true)]
    public SectorBankAccount Account;
}
