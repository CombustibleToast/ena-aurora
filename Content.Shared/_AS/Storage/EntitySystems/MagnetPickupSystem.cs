using Content.Shared._AS.Storage.Components;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components;
using Content.Shared.Trigger.Systems;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Shared._AS.Storage.EntitySystems;

/// <summary>
/// <see cref="MagnetPickupComponent"/>
/// </summary>
public sealed partial class MagnetPickupSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _netMan = default!;
    [Dependency] private SharedPowerReceiverSystem _power = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private ItemToggleSystem _itemToggle = default!;
    [Dependency] private SharedAudioSystem _audio = default!; 
    [Dependency] private SharedContainerSystem _containerSystem = default!; 
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedStorageSystem _storage = default!;
    [Dependency] private EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private SharedItemSystem _item = default!; // Frontier
    [Dependency] private TriggerSystem _trigger = default!;
    [Dependency] private EntityQuery<PhysicsComponent> _physicsQuery = default!;

    private const int MaxEntitiesToInsert = 15; // Frontier

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MagnetPickupComponent, ExaminedEvent>(OnExamined); // Frontier
        SubscribeLocalEvent<MagnetPickupComponent, GetVerbsEvent<AlternativeVerb>>(AddToggleMagnetVerb); // Frontier

        SubscribeLocalEvent<MagnetPickupComponent, AttemptTriggerEvent>(MagnetDrawStorage);
        SubscribeLocalEvent<MagnetPickupComponent, ItemToggleActivateAttemptEvent>(MagnetToggle);
        SubscribeLocalEvent<MagnetPickupComponent, ItemToggleDeactivateAttemptEvent>(MagnetToggle);
        SubscribeLocalEvent<MagnetPickupComponent, SignalReceivedEvent>(OnSignalReceived);
        SubscribeLocalEvent<MagnetPickupComponent, PowerChangedEvent>(OnPowerChanged);
    }

    /// <summary>
    /// Handles the receiving of signals when connected to buttons, timers, etc.
    /// Ports control:
    ///     - Off
    ///     - On
    ///     - Toggle between On/Off
    ///     - Do a magnet pull
    /// </summary>
    /// <param name="ent">The entity & magnet</param>
    /// <param name="args">SignalReceivedEvent arguments</param>
    private void OnSignalReceived(Entity<MagnetPickupComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port == ent.Comp.OffPort)
            ForceStatus(ent, false);
        else if (args.Port == ent.Comp.OnPort)
            ForceStatus(ent, true);
        else if (args.Port == ent.Comp.TogglePort)
            ToggleInternal(ent);
        else if (args.Port == ent.Comp.PullPort)
            SolveStorage(ent);
    }

    /// <summary>
    /// Called when power to the magnet changes
    /// This only has effect if the components RequirePower is true
    /// </summary>
    /// <param name="ent">The entity & magnet</param>
    /// <param name="args">PowerChangedEvent arguments</param>
    private void OnPowerChanged(Entity<MagnetPickupComponent> ent, ref PowerChangedEvent args)
    {
        if (!ent.Comp.RequirePower)
            return;

        if(ent.Comp.MagnetEnabled && !args.Powered)
        {
            ent.Comp.TurnOnWhenPowered = true;
            ForceStatus(ent, false);
        }
        else if (args.Powered && ent.Comp.TurnOnWhenPowered)
        {
            ent.Comp.TurnOnWhenPowered = false;
            ForceStatus(ent, true);
        }
    }

    /// <summary>
    /// Check if the magnet is powered
    /// </summary>
    /// <param name="ent">The entity & magnet</param>
    /// <returns>True if there is power</returns>
    private bool CheckForPower(Entity<MagnetPickupComponent> ent)
    {
        if (!_power.IsPowered(ent.Owner))
            return false;

        return true;
    }

    /// <summary>
    /// Used for if this component is using the ItemToggleComponent instead
    /// This version exists so that if the magnet has no power, the item won't toggle on.
    /// </summary>
    /// <param name="ent">The entity & magnet</param>
    /// <param name="args">ItemToggleActivateAttemptEvent arguments</param>
    private void MagnetToggle(Entity<MagnetPickupComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (!ent.Comp.UseItemToggle || !CheckForPower(ent))
        {
            args.Cancelled = true;
            return;
        }

        ToggleMagnet(ent.Owner, ent.Comp);
    }

    /// <summary>
    /// Used for if this component is using the ItemToggleComponent instead
    /// </summary>
    /// <param name="ent">The entity & magnet</param>
    /// <param name="args">ItemToggleActivateAttemptEvent arguments</param>
    private void MagnetToggle(Entity<MagnetPickupComponent> ent, ref ItemToggleDeactivateAttemptEvent args)
    {
        if (!ent.Comp.UseItemToggle)
        {
            args.Cancelled = true;
            return;
        }

        ToggleMagnet(ent.Owner, ent.Comp);
    }

    /// <summary>
    /// Used for forcing the magnet to be off or on either through ItemToggleSystem or directly
    /// </summary>
    /// <param name="ent">The entity & magnet</param>
    /// <param name="status">Whether the magnet should be on or off</param>
    private void ForceStatus(Entity<MagnetPickupComponent> ent, bool status)
    {
        if (ent.Comp.UseItemToggle && TryComp<ItemToggleComponent>(ent.Owner, out var toggle))
        {
            if (status)
                _itemToggle.TryActivate((ent.Owner, toggle));
            else
                _itemToggle.TryDeactivate((ent.Owner, toggle));

            return;
        }

        ent.Comp.MagnetEnabled = (ent.Comp.RequirePower && !CheckForPower(ent)) ? false : status;

        var toggleEvent = new MagnetToggleEvent(ent.Comp.MagnetEnabled);
        RaiseLocalEvent(ent.Owner, ref toggleEvent);

        if (TryComp<TimerTriggerComponent>(ent.Owner, out var timer))
            ToggleTimer((ent.Owner, timer), ent.Comp.MagnetEnabled);

        Dirty(ent.Owner, ent.Comp);
    }

    /// <summary>
    /// Called internally to toggle the magnet
    /// This is used to determine if we need to go through ItemToggleSystem or do it directly
    /// </summary>
    /// <param name="ent">The entity & magnet</param>
    private void ToggleInternal(Entity<MagnetPickupComponent> ent)
    {
        if (ent.Comp.UseItemToggle && TryComp<ItemToggleComponent>(ent.Owner, out var toggle))
            _itemToggle.Toggle((ent.Owner, toggle));
        else
            ToggleMagnet(ent.Owner, ent.Comp);
    }

    /// <summary>
    /// Toggles the magnet
    /// This also toggles the magnet timer & calls an event for other systems that
    /// need to udpate visuals or do other things affected by the toggle.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="comp"></param>
    public void ToggleMagnet(EntityUid uid, MagnetPickupComponent comp)
    {
        comp.MagnetEnabled = (comp.RequirePower && !CheckForPower((uid, comp))) ? false : !comp.MagnetEnabled;

        var toggleEvent = new MagnetToggleEvent(comp.MagnetEnabled);
        RaiseLocalEvent(uid, ref toggleEvent);

        // Making sure we ACTUALLY have the timer
        EnsureComp<TimerTriggerComponent>(uid, out var timer);
        ToggleTimer((uid, timer), comp.MagnetEnabled);

        Dirty(uid, comp);
    }

    /// <summary>
    /// Called when the Timer on a magnet goes off
    /// </summary>
    /// <param name="ent">The entity & magnet component coupling</param>
    /// <param name="args">AttemptTriggerEvent's arguments.</param>
    public void MagnetDrawStorage(Entity<MagnetPickupComponent> ent, ref AttemptTriggerEvent args)
    {
        if (_netMan.IsClient)
            return;

        SolveStorage(ent);
    }

    /// <summary>
    /// Toggles if the time should be active or not.
    /// GEts toggled when the magnet is toggled on or off.
    /// </summary>
    /// <param name="ent">The entity & timer coupling</param>
    /// <param name="toggle">If the timer should be on or off</param>
    private void ToggleTimer(Entity<TimerTriggerComponent?> ent, bool toggle)
    {
        if (toggle)
            _trigger.ActivateTimerTrigger(ent);
        else
            _trigger.StopTimerTrigger(ent);
    }
}
