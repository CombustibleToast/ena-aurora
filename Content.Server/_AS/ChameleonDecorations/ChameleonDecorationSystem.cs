using Content.Shared._AS.ChameleonDecorations;
using Content.Shared.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.ChameleonDecorations;

public sealed partial class ChameleonDecorationSystem : SharedChameleonDecorationSystem
{
    [Dependency] private IPrototypeManager _prototypes = null!;
    [Dependency] private SharedUserInterfaceSystem _ui = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChameleonDecorationComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChameleonDecorationComponent, BeforeActivatableUIOpenEvent>(OnBeforeUiOpen);
        SubscribeLocalEvent<ChameleonDecorationComponent, ChameleonDecorationSelectMessage>(OnSelected);
    }

    private void OnMapInit(Entity<ChameleonDecorationComponent> ent, ref MapInitEvent args)
    {
        ApplyAppearance(ent);
    }

    private void OnBeforeUiOpen(Entity<ChameleonDecorationComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUi(ent);
    }

    private void OnSelected(Entity<ChameleonDecorationComponent> ent, ref ChameleonDecorationSelectMessage args)
    {
        if (ent.Comp.Selected == args.Selected ||
            !_prototypes.TryIndex(args.Selected, out var prototype) ||
            !IsValidTarget(prototype, ent.Comp.Group))
            return;

        ent.Comp.Selected = args.Selected;
        Dirty(ent);
        ApplyAppearance(ent);
        UpdateUi(ent);
    }

    private void UpdateUi(Entity<ChameleonDecorationComponent> ent)
    {
        _ui.SetUiState(ent.Owner,
            ChameleonDecorationUiKey.Key,
            new ChameleonDecorationUiState(ent.Comp.Group, ent.Comp.Selected));
    }
}
