using Content.Shared._Starfall.Particles.Visuals;
using Content.Shared.Damage.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._Starfall.Particles.Visuals;

/// <summary>
/// Rolls machine sparks on the server, plays their sound, and exposes the tile to an ignition hotspot.
/// </summary>
public sealed partial class SparksOnDamageSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = null!;
    [Dependency] private IRobustRandom _random = null!;
    [Dependency] private SharedTransformSystem _transform = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SparksOnDamageComponent, DamageDealtEvent>(OnDamageDealt);
        SubscribeLocalEvent<SparksOnDamageComponent, TriggerSparksEvent>(OnTriggerSparks);
    }

    private void OnDamageDealt(Entity<SparksOnDamageComponent> ent, ref DamageDealtEvent args)
    {
        if (!args.Damage.AnyPositive() || !_random.Prob(Math.Clamp(ent.Comp.Chance, 0f, 1f)))
            return;

        Spark(ent);
    }

    private void OnTriggerSparks(Entity<SparksOnDamageComponent> ent, ref TriggerSparksEvent args)
    {
        Spark(ent);
    }

    private void Spark(Entity<SparksOnDamageComponent> ent)
    {
        _audio.PlayPvs(ent.Comp.Sound, ent.Owner);

        var xform = Transform(ent.Owner);
        Spawn(ent.Comp.IgnitionSource, xform.Coordinates);

        var coordinates = _transform.GetMapCoordinates(ent.Owner, xform);
        RaiseNetworkEvent(new SparksOnDamageEvent(coordinates, ent.Comp.Effect.Id, ent.Comp.LightColor, ent.Comp.LightRadius, ent.Comp.LightEnergy, ent.Comp.LightDuration), Filter.Pvs(coordinates));
    }
}
