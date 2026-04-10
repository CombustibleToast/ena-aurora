using System.Threading; // Coyote
using Content.Shared.Implants;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components; // Coyote
using Content.Shared.Popups;
using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.Timing; // Coyote

namespace Content.Shared.Trigger.Systems;

public sealed partial class TriggerOnMobstateChangeSystem : TriggerOnXSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnMobstateChangeComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<TriggerOnMobstateChangeComponent, SuicideEvent>(OnSuicide);

        SubscribeLocalEvent<TriggerOnMobstateChangeComponent, ImplantRelayEvent<MobStateChangedEvent>>(OnMobStateRelay);
        SubscribeLocalEvent<TriggerOnMobstateChangeComponent, ImplantRelayEvent<SuicideEvent>>(OnSuicideRelay);
        SubscribeLocalEvent<TriggerOnMobstateChangeComponent, ImplantRelayEvent<ReTriggerRattleImplantEvent>>(OnFtlArriveRelay); // Coyote
    }

    private void OnMobStateChanged(EntityUid uid, TriggerOnMobstateChangeComponent component, MobStateChangedEvent args)
    {
        component.RattleCancelToken.Cancel(); // Coyote
        component.RattleCancelToken = new CancellationTokenSource(); // Coyote
        if (!component.MobState.Contains(args.NewMobState))
            return;

        TryRunTrigger(
            uid,
            component,
            args.Target,
            args.NewMobState,
            args.Origin); // Coyote
    }

    private void TryRunTrigger(
        EntityUid uid,
        TriggerOnMobstateChangeComponent component,
        EntityUid changedStateMobUid,
        MobState coolState,
        EntityUid? stateChangerUid = null,
        bool retry = false)
    {
        //This chains Mobstate Changed triggers with OnUseTimerTrigger if they have it
        //Very useful for things that require a mobstate change and a timer
        if (TryComp<OnUseTimerTriggerComponent>(uid, out var timerTrigger))
        {
            HandleTimerTrigger(
                uid,
                stateChangerUid,
                timerTrigger.Delay,
                timerTrigger.BeepInterval,
                timerTrigger.InitialBeepDelay,
                timerTrigger.BeepSound);
        }
        else
        {
            Dictionary<string, object> extraData = new() // Coyote
            {
                { "isRetry", retry }
            };
            Trigger.Trigger(uid, component.TargetMobstateEntity ? uid : stateChangerUid, component.KeyOut, extras: extraData);
        }

        // Coyote: Then run it again
        component.RattleCancelToken.Cancel();
        component.RattleCancelToken = new CancellationTokenSource();
        Robust.Shared.Timing.Timer.Spawn(component.RattleRefireDelay, () => CheckAndTryRefire(uid, component, changedStateMobUid), component.RattleCancelToken.Token);
    }

    // Coyote
    /// <summary>
    /// Check if the trigger can be retriggered and does so if possible
    /// </summary>
    private void CheckAndTryRefire(
        EntityUid uid,
        TriggerOnMobstateChangeComponent component,
        EntityUid changedStateMobUid)
    {
        if (!Exists(uid)
            || !Exists(changedStateMobUid))
            return;
        if (Deleted(uid)
            || Deleted(changedStateMobUid))
            return;
        if (!HasComp<MobStateComponent>(changedStateMobUid))
            return;
        var stat = Comp<MobStateComponent>(changedStateMobUid).CurrentState;
        if (component.MobState.Contains(stat))
        {
            TryRunTrigger(
                uid,
                component,
                changedStateMobUid,
                stat,
                null,
                true);
        }
    }

    /// <summary>
    /// Checks if the user has any implants that prevent suicide to avoid some cheesy strategies
    /// Prevents suicide by handling the event without killing the user
    /// TODO: This doesn't seem to work at the moment as the event is never checked for being handled.
    /// </summary>
    private void OnSuicide(EntityUid uid, TriggerOnMobstateChangeComponent component, SuicideEvent args)
    {
        if (args.Handled)
            return;

        if (!component.PreventSuicide)
            return;

        _popup.PopupClient(Loc.GetString("suicide-prevented"), args.Victim);
        args.Handled = true;
    }

    private void OnSuicideRelay(EntityUid uid, TriggerOnMobstateChangeComponent component, ImplantRelayEvent<SuicideEvent> args)
    {
        if (args.Event.Handled)
            return;

        if (!component.PreventSuicide)
            return;

        _popup.PopupClient(Loc.GetString("suicide-prevented"), args.Event.Victim);
        args.Event.Handled = true;
    }

    // Coyote
    /// <summary>
    /// When ftl arrives, try to retrigger their medical alerts
    /// </summary>
    private void OnFtlArriveRelay(EntityUid uid,
        TriggerOnMobstateChangeComponent component,
        ImplantRelayEvent<ReTriggerRattleImplantEvent> args)
    {
        TryRunTrigger(
            uid,
            component,
            args.Event.Implanted,
            args.Event.CurrentState,
            null);
    }
}
