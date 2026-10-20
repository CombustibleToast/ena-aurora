using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Weapons.Melee.EnergySword;

public sealed partial class EnergySwordSystem
{
    [Dependency] private SharedContainerSystem _storageContainers = null!;
    [Dependency] private ItemToggleSystem _storageToggle = null!;
    [Dependency] private SharedHandsSystem _storageHands = null!;

    private void InitializeEnergyWeaponStorage()
    {
        SubscribeLocalEvent<EnergySwordComponent, ContainerGettingInsertedAttemptEvent>(OnContainerInsertAttempt);
        SubscribeLocalEvent<EnergySwordComponent, ItemToggleActivateAttemptEvent>(OnActivateAttemptInContainer);
    }

    /// <summary>
    /// Prevents active energy weapons from entering storage without affecting their toggle state.
    /// </summary>
    private void OnContainerInsertAttempt(EntityUid uid, EnergySwordComponent component, ContainerGettingInsertedAttemptEvent args)
    {
        if (_storageHands.TryGetHand(args.Container.Owner, args.Container.ID, out _))
            return;

        if (_storageToggle.IsActivated(uid))
            args.Cancel();
    }

    /// <summary>
    /// Prevents stored energy weapons from activating
    /// </summary>
    private void OnActivateAttemptInContainer(
        Entity<EnergySwordComponent> ent,
        ref ItemToggleActivateAttemptEvent args)
    {
        if (args.Cancelled || !_storageContainers.TryGetContainingContainer((ent.Owner, null, null), out var container))
            return;

        if (_storageHands.TryGetHand(container.Owner, container.ID, out _))
            return;

        args.Cancelled = true;
    }
}
