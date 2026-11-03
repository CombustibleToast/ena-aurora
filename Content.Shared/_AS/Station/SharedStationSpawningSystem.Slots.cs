using Content.Shared.Inventory;

namespace Content.Shared.Station;

// Loadouts can fill multiple slots with the same slot flags.
public abstract partial class SharedStationSpawningSystem
{
    private string GetLoadoutSlot(EntityUid entity, EntityUid item, SlotDefinition requested, SlotDefinition[] slots)
    {
        if (requested.SlotFlags == SlotFlags.NONE || !InventorySystem.TryGetSlotEntity(entity, requested.Name, out _))
            return requested.Name;

        foreach (var slot in slots)
        {
            if (slot.SlotFlags == requested.SlotFlags && !InventorySystem.TryGetSlotEntity(entity, slot.Name, out _) && InventorySystem.CanEquip(entity, item, slot.Name, out _, slot))
                return slot.Name;
        }

        return requested.Name;
    }
}
