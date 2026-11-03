using Content.Shared.Clothing.Components;
using Content.Shared.Interaction;

namespace Content.Client.Inventory;

public sealed partial class StrippableBoundUserInterface
{
    /// <summary>
    /// Handles toggling toggleable clothing in the strip menu via alt-click
    /// </summary>
    private bool TryToggleClothingFromSlot(EntityUid item)
    {
        if (!EntMan.HasComponent<ToggleableClothingComponent>(item))
            return false;

        EntMan.RaisePredictiveEvent(new InteractInventorySlotEvent(EntMan.GetNetEntity(item), altInteract: true));
        return true;
    }
}
