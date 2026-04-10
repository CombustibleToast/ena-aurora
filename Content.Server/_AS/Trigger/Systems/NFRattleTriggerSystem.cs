using Content.Server.Explosion.EntitySystems;
using Content.Server.Radio.EntitySystems;
using Content.Shared._AS.Traits;
using Content.Shared.Humanoid;
using Content.Shared.Implants.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Radio;
using Content.Shared.Station;
using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.Trigger.Systems;

public sealed class NFRattleTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly RadioSystem _radioSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RattleOnTriggerComponent, TriggerEvent>(HandleRattleTrigger);
    }

    private void HandleRattleTrigger(EntityUid uid, RattleOnTriggerComponent component, TriggerEvent args)
    {
        if (!TryComp<SubdermalImplantComponent>(uid, out var implanted))
            return;

        if (implanted.ImplantedEntity == null)
            return;
        // Coyote
        if (!TryComp<MobStateComponent>(implanted.ImplantedEntity, out var mobstate)
            || mobstate.CurrentState == MobState.Alive)
            return;


        // Gets location of the implant
        var ownerXform = Transform(uid);
        var pos = ownerXform.MapPosition;
        var x = (int)pos.X;
        var y = (int)pos.Y;
        var posText = $"({x}, {y})";

        // Frontier: Gets station location of the implant
        var station = _station.GetOwningStation(uid);
        var stationText = station is null ? null : $"{Name(station.Value)} ";

        if (stationText == null)
            stationText = "";

        // Frontier: Gets species of the implant user
        var speciesText = $"";
        if (TryComp<HumanoidAppearanceComponent>(implanted.ImplantedEntity, out var species))
        {

            if (HasComp<ReplicantComponent>(implanted.ImplantedEntity)) // AS: Replika
            {
                speciesText = $" ({Loc.GetString("species-name-replicant", ("species", species!.Species))})";  // AS: Replika
            }
            else
            {
                speciesText = $" ({species!.Species})";
            }
        }
        // Start Coyote
        string localeKey = component.Messages[mobstate.CurrentState];

        var message = Loc.GetString(
            localeKey,
            ("user", implanted.ImplantedEntity.Value),
            ("specie", speciesText),
            ("grid", stationText!),
            ("position", posText));

        _radioSystem.SendRadioMessage(
            uid,
            message,
            _prototypeManager.Index<RadioChannelPrototype>(component.RadioChannel),
            uid);
        // End Coyote
        args.Handled = true;
    }
}
