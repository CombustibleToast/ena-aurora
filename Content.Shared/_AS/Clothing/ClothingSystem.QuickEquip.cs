using System.Linq;
using Content.Shared.Clothing.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;

namespace Content.Shared.Clothing.EntitySystems;

// Quick equip fills similar empty slots before replacing their contents.
public abstract partial class ClothingSystem
{
    private bool ShouldDeferQuickEquip(
        Entity<ClothingComponent> clothing,
        Entity<InventoryComponent, HandsComponent> user,
        SlotDefinition requested)
    {
        if (requested.SlotFlags == SlotFlags.NONE || !_invSystem.TryGetSlotEntity(user, requested.Name, out _, user))
            return false;

        return user.Comp1.Slots.Any(slot => slot.SlotFlags == requested.SlotFlags && !_invSystem.TryGetSlotEntity(user, slot.Name, out _, user) && _invSystem.CanEquip(user, clothing, slot.Name, out _, slot, user, clothing));
    }
}
