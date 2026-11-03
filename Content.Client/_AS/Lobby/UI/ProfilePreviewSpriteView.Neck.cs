using Content.Shared.Inventory;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;

namespace Content.Client.Lobby.UI.ProfileEditorControls;

// Keep both neck preview slots in selection order.
public sealed partial class ProfilePreviewSpriteView
{
    private InventorySystem? _inventorySystem;

    private void PrepareNeckLoadoutPreview(LoadoutPrototype loadout, ref bool clearedNeck)
    {
        if (clearedNeck)
            return;

        var hasNeck = !string.IsNullOrEmpty(((IEquipmentLoadout) loadout).GetGear("neck"));
        if (_prototypeManager.Resolve(loadout.StartingGear, out var startingGear))
            hasNeck |= !string.IsNullOrEmpty(((IEquipmentLoadout) startingGear).GetGear("neck"));

        if (!hasNeck)
            return;

        var inventorySys = _inventorySystem ??= EntMan.System<InventorySystem>();
        foreach (var slotName in new[] { "neck", "neck2" })
        {
            if (inventorySys.TryUnequip(PreviewDummy, slotName, out var item, silent: true, force: true))
                EntMan.DeleteEntity(item.Value);
        }

        clearedNeck = true;
    }
}
