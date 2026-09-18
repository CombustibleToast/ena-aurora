using Content.Server.Actions;
using Content.Shared.Inventory;
using Content.Shared.Toggleable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.Togglable;

/// <summary>
/// Handles the action for toggling belt visuals above or below outer clothing.
/// </summary>
public sealed partial class ToggleBeltLayerSystem : EntitySystem
{
    [Dependency] private ActionsSystem _actions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private InventorySystem _inventory = default!;

    private static readonly EntProtoId ToggleAction = "ActionToggleBeltLayer";
    private static readonly SoundSpecifier BeltEquipSound = new SoundPathSpecifier("/Audio/Items/belt_equip.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleBeltLayerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ToggleBeltLayerComponent, ToggleBeltLayerActionEvent>(OnToggle);
    }

    private void OnMapInit(Entity<ToggleBeltLayerComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ToggleActionEntity, ToggleAction);
    }

    private void OnToggle(Entity<ToggleBeltLayerComponent> ent, ref ToggleBeltLayerActionEvent args)
    {
        ent.Comp.BeltOverOuterClothing = !ent.Comp.BeltOverOuterClothing;
        Dirty(ent);

        if (_inventory.TryGetSlotEntity(ent, "belt", out _))
            _audio.PlayPvs(BeltEquipSound, ent);

        args.Handled = true;
    }
}
