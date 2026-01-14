// Aurora Song - AS Medical Bounty System
// Based on New Frontier Station 14's Medical Bounty System
// Original implementation: https://github.com/new-frontiers-14/frontier-station-14
using JetBrains.Annotations;
using Content.Client._AS.Medical.UI;
using Content.Shared._AS.Medical;
using Robust.Client.UserInterface;

namespace Content.Client._AS.Medical.BUI;

/// <summary>
/// Aurora Song: Bound user interface for AS medical bounty redemption terminals
/// </summary>
[UsedImplicitly]
public sealed class ASMedicalBountyRedemptionBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ASMedicalBountyRedemptionMenu? _menu;

    public ASMedicalBountyRedemptionBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        if (_menu == null)
        {
            _menu = this.CreateWindow<ASMedicalBountyRedemptionMenu>();
            _menu.SellRequested += SendBountyMessage;
        }
    }

    private void SendBountyMessage()
    {
        SendMessage(new RedeemASMedicalBountyMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState message)
    {
        base.UpdateState(message);

        if (message is not ASMedicalBountyRedemptionUIState state)
            return;

        _menu?.UpdateState(state);
    }
}
