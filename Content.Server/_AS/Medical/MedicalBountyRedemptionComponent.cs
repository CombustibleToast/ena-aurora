// Aurora Song - AS Medical Bounty System
// Based on New Frontier Station 14's Medical Bounty System
// Original implementation: https://github.com/new-frontiers-14/frontier-station-14
using Content.Shared._NF.Bank.Components;
using Robust.Shared.Audio;

namespace Content.Server._AS.Medical;

/// <summary>
/// Aurora Song: Component for machines that redeem AS medical bounties
/// </summary>
[RegisterComponent]
public sealed partial class ASMedicalBountyRedemptionComponent : Component
{
    /// <summary>
    /// The name of the container that holds medical bounties to be redeemed.
    /// </summary>
    [DataField(required: true)]
    public string BodyContainer;

    /// <summary>
    /// The sound that plays when a medical bounty is redeemed successfully.
    /// </summary>
    [DataField]
    public SoundSpecifier RedeemSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    /// <summary>
    /// The sound that plays when a medical bounty is unsuccessfully redeemed.
    /// </summary>
    [DataField]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_sigh.ogg");

    [DataField]
    public Dictionary<SectorBankAccount, float> TaxAccounts = new();
}
