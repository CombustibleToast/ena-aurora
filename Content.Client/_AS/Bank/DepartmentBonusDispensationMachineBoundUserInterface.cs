// Aurora Song - Department Bonus Dispensation Machine BUI

using Content.Client._AS.Bank.UI;
using Content.Shared._AS.Bank;
using Robust.Client.UserInterface;

namespace Content.Client._AS.Bank;

public sealed class DepartmentBonusDispensationMachineBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private DepartmentBonusDispensationMachine? _window;

    public DepartmentBonusDispensationMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<DepartmentBonusDispensationMachine>();
        _window.OnEjectPressed += OnEjectPressed;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not DepartmentBonusDispensationMachineBoundUserInterfaceState taxState)
            return;

        _window?.UpdateState(taxState);
    }

    private void OnEjectPressed()
    {
        SendMessage(new DepartmentBonusDispensationMachineEjectMessage());
    }
}
