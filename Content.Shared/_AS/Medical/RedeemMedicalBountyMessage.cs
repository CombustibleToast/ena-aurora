// Aurora Song - AS Medical Bounty System
// Separate implementation from NF Medical Bounty System
using Robust.Shared.Serialization;

namespace Content.Shared._AS.Medical;

/// <summary>
/// Aurora Song: Message sent when redeeming an AS medical bounty
/// </summary>
[Serializable, NetSerializable]
public sealed class RedeemASMedicalBountyMessage : BoundUserInterfaceMessage
{
    public RedeemASMedicalBountyMessage()
    {
    }
}
