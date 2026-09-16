using Content.Client.Inventory;
using Content.Shared.Inventory;
using Content.Shared.Toggleable;
using Robust.Client.GameObjects;

namespace Content.Client.Clothing;

/// <summary>
/// Handles appearance toggle support for rendering belts above/below outer clothing.
/// </summary>
public sealed partial class ClientClothingSystem
{
    private const string BeltSlot = "belt";
    private const string OuterClothingSlot = "outerClothing";

    private void InitializeBeltLayerToggle()
    {
        SubscribeLocalEvent<ToggleBeltLayerComponent, AfterAutoHandleStateEvent>(OnAppearanceToggleState);
    }

    private void OnAppearanceToggleState(Entity<ToggleBeltLayerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp(ent, out InventoryComponent? inventory) || !_inventorySystem.TryGetSlotEntity(ent, BeltSlot, out var belt, inventory))
            return;

        RenderEquipment(ent, belt.Value, BeltSlot, inventory);
    }

    private void AdjustBeltLayerBookmark(EntityUid equipee, string slot, ref string bookmarkSlot)
    {
        if (slot == BeltSlot && CompOrNull<ToggleBeltLayerComponent>(equipee)?.BeltOverOuterClothing == true)
        {
            bookmarkSlot = OuterClothingSlot;
        }
    }

    private void AdjustBeltLayerInsertionIndex(EntityUid equipee, SpriteComponent sprite, string slot, InventorySlotsComponent inventorySlots, ref int index)
    {
        if (slot != BeltSlot || CompOrNull<ToggleBeltLayerComponent>(equipee)?.BeltOverOuterClothing != true || !inventorySlots.VisualLayerKeys.TryGetValue(OuterClothingSlot, out var outerLayers))
            return;

        // Insert after every visual belonging to outer clothing, not just after its bookmark.
        foreach (var outerLayer in outerLayers)
        {
            if (_sprite.LayerMapTryGet((equipee, sprite), outerLayer, out var outerIndex, false))
                index = Math.Max(index, outerIndex);
        }
    }
}
