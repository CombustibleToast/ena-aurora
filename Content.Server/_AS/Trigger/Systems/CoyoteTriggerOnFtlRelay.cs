using Content.Shared.Implants;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components.Triggers;

namespace Content.Server._AS.Trigger.Systems;

public sealed class CoyoteTriggerOnFtlRelay : TriggerOnXSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnMobstateChangeComponent, ImplantRelayEvent<ReTriggerRattleImplantEvent>>(OnFtlArriveRelay);
    }

    // Coyote
    /// <summary>
    /// When ftl arrives, try to retrigger their medical alerts
    /// </summary>
    private void OnFtlArriveRelay(EntityUid uid,
        TriggerOnMobstateChangeComponent component,
        ImplantRelayEvent<ReTriggerRattleImplantEvent> args)
    {
        Trigger.Trigger(uid, args.Event.Implanted, component.KeyOut);
    }
}
