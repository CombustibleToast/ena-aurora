using Content.Shared._AS.Storage.Components;
using Content.Shared.Item;
using Content.Shared.Storage;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Shared._AS.Storage.EntitySystems;

/// <summary>
/// <see cref="MagnetPickupComponent"/>
/// This file in particular handles the actual pickup related functions
/// </summary>
public sealed partial class MagnetPickupSystem : EntitySystem
{
    /// <summary>
    /// Figures out what type of storage system is connected to the entity before handling
    /// pulling in items.
    /// This prioritises StorageComponent over ContainerContainer.
    /// </summary>
    /// <param name="ent">The entity the magnet is attached to & component</param>
    public void SolveStorage(Entity<MagnetPickupComponent> ent)
    {
        if (_timing.ApplyingState)
            return;

        if (TryComp<StorageComponent>(ent.Owner, out var storage))
            PullInItemsStorage(ent, storage);
        else if (TryComp<ContainerManagerComponent>(ent.Owner, out var container))
            PullInItemsContainer(ent, container);
    }

    /// <summary>
    /// Gathers a Hashset of the nearby items
    /// </summary>
    /// <param name="uid">The main searcher</param>
    /// <param name="range">The max range for the search</param>
    /// <param name="flags"></param>
    /// <param name="whitelist">What items are allowed</param>
    /// <param name="pickupLimit">The max amount of items that can be found</param>
    /// <returns></returns>
    public HashSet<EntityUid> ValidPickups(
        EntityUid uid,
        float range = 1,
        LookupFlags flags = LookupFlags.All,
        EntityWhitelist? whitelist = null,
        int pickupLimit = MaxEntitiesToInsert)
    {
        var pickups = new HashSet<EntityUid>();

        int count = 0;
        foreach (var near in _lookup.GetEntitiesInRange(uid, range, flags))
        {
            if (count > pickupLimit)
                break;

            if (near == uid)
                continue;

            if (!_physicsQuery.TryGetComponent(near, out var physics) || physics.BodyStatus != BodyStatus.OnGround)
                continue;

            if (!TryComp<ItemComponent>(near, out var item))
                continue;

            if (whitelist != null && _whitelistSystem.IsWhitelistFail(whitelist, near))
                continue;

            pickups.Add(near);

            count++;
        }

        return pickups;
    }

    /// <summary>
    /// Handles drawing in nearby items into the storage
    /// This version works with ContainerManagerComponent
    /// </summary>
    /// <param name="ent">The entity that has the magnet & component</param>
    /// <param name="storage">The storage attached to the magnet entity</param>
    public void PullInItemsContainer(Entity<MagnetPickupComponent> ent, ContainerManagerComponent container)
    {
        var comp = ent.Comp;

        var xform = Transform(ent.Owner);
        var uid = ent.Owner;

        var finalCoords = xform.Coordinates;
        var moverCoords = _transform.GetMoverCoordinates(uid, xform);

        if (!container.Containers.TryGetValue(comp.ContainerFocus, out var targetContainer))
            return;

        int collected = 0;

        var pickups = ValidPickups(uid, comp.Range, LookupFlags.Dynamic | LookupFlags.Sundries);
        foreach (var item in pickups)
        {
            // TODO: Probably move this to storage somewhere when it gets cleaned up
            // TODO: This sucks but you need to fix a lot of stuff to make it better
            // the problem is that stack pickups delete the original entity, which is fine, but due to
            // game state handling we can't show a lerp animation for it.
            var nearXform = Transform(item);
            var nearMap = _transform.GetMapCoordinates(item, xform: nearXform);
            var nearCoords = _transform.ToCoordinates(moverCoords.EntityId, nearMap);

            if (!_containerSystem.CanInsert(item, targetContainer))
            {
                pickups.Remove(item);
                continue;
            }


            // Play pickup animation for the entity.
            _storage.PlayPickupAnimation(item, nearCoords, finalCoords, nearXform.LocalRotation);
            _audio.PlayPredicted(comp.PickupSound, uid, null);

            // Try to insert it (if this fails, cry)
            if (!_containerSystem.Insert(item, targetContainer))
                throw new InvalidOperationException("Container insertion failed but CanInsert returned true");

            collected++;

            var args = new MagnetPickupEvent(item);
            RaiseLocalEvent(uid, ref args);
        }

        var args2 = new FinishMagnetPickupEvent(collected, pickups);
        RaiseLocalEvent(uid, ref args2);
    }

    /// <summary>
    /// Handles drawing in nearby items into the storage
    /// This version works with StorageComponent
    /// </summary>
    /// <param name="ent">The entity that has the magnet & component</param>
    /// <param name="storage">The storage attached to the magnet entity</param>
    public void PullInItemsStorage(Entity<MagnetPickupComponent> ent, StorageComponent storage)
    {
        var comp = ent.Comp;

        var xform = Transform(ent.Owner);
        var coordinateEntity = xform.ParentUid.IsValid() ? xform.ParentUid : ent.Owner;

        var uid = ent.Owner;

        // Frontier: run conservative space estimations, cut down on space checks
        var slotCount = _storage.GetCumulativeItemAreas((uid, storage)); // Frontier
        var totalSlots = storage.Grid.GetArea();
        if (slotCount >= totalSlots)
            return;
        // End Frontier

        var finalCoords = xform.Coordinates;
        var moverCoords = _transform.GetMoverCoordinates(uid, xform);

        int collected = 0;

        var pickups = ValidPickups(uid, comp.Range, LookupFlags.Dynamic | LookupFlags.Sundries, storage.Whitelist);
        foreach (var item in pickups)
        {
            if (!TryComp<ItemComponent>(item, out var itemC))
                continue;

            var itemSize = _item.GetItemShape((item, itemC)).GetArea();
            if (itemSize > totalSlots - slotCount)
                break;

            // Our animation coordinate information
            var itemxForm = Transform(item);
            var itemPos = _transform.GetMapCoordinates(item, xform: itemxForm);
            var initialPosition = _transform.ToCoordinates(coordinateEntity, itemPos);

            if (!_storage.Insert(uid, item, out var stacked, storageComp: storage, playSound: false))
                continue;

            slotCount += itemSize; // Frontier: adjust size (assume it's in a new slot)
            collected++;

            var args = new MagnetPickupEvent(item);
            RaiseLocalEvent(uid, ref args);

            _storage.PlayPickupAnimation(item, initialPosition, xform.Coordinates, itemxForm.LocalRotation, ent.Owner);
            _audio.PlayPredicted(comp.PickupSound, uid, null);
        }

        var args2 = new FinishMagnetPickupEvent(collected, pickups);
        RaiseLocalEvent(uid, ref args2);
    }
}
