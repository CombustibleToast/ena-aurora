using Content.Client.Clothing.UI;
using Content.Shared._AS.ChameleonDecorations;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._AS.ChameleonDecorations;

[UsedImplicitly]
public sealed class ChameleonDecorationBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private ChameleonMenu? _menu;

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<ChameleonMenu>();
        _menu.Title = Loc.GetString("chameleon-decoration-menu-title");
        _menu.OnIdSelected += OnSelected;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not ChameleonDecorationUiState decorationState)
            return;

        var system = EntMan.System<ChameleonDecorationSystem>();
        _menu?.UpdateState(system.GetTargets(decorationState.Group), decorationState.Selected);
    }

    private void OnSelected(string selected)
    {
        SendMessage(new ChameleonDecorationSelectMessage(selected));
    }
}
