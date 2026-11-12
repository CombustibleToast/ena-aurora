using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared._Starfall.Particles.Visuals;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Robust.Shared.Player;

namespace Content.Server._Starfall.Particles.Visuals;

/// <summary>
/// When cold air is exhaled raise <see cref="ColdBreathParticleEvent"/> to the client.
/// </summary>
public sealed partial class ColdBreathParticleSystem : EntitySystem
{
    // We dont have humidity to calculate exact point where cold breath should begin so im just doing 10c
    private const float ColdBreathTemperature = Atmospherics.T0C + 10f;
    // Condensation clouds don't work in low pressure environments
    private const float MinimumColdBreathPressure = Atmospherics.HazardLowPressure;

    [Dependency] private InventorySystem _inventory = null!;
    [Dependency] private SharedInternalsSystem _internals = null!;
    [Dependency] private SharedTransformSystem _transform = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RespiratorComponent, ExhaledGasEvent>(OnExhaled);
    }

    private void OnExhaled(Entity<RespiratorComponent> ent, ref ExhaledGasEvent args)
    {
        if (args.Gas.Temperature > ColdBreathTemperature || args.Gas.Pressure <= MinimumColdBreathPressure)
            return;

        if (_internals.AreInternalsWorking(ent.Owner))
            return;

        var slots = _inventory.GetSlotEnumerator((ent.Owner, null), SlotFlags.MASK | SlotFlags.HEAD);
        while (slots.NextItem(out var worn, out var slot))
        {
            if ((slot.SlotFlags & SlotFlags.MASK) != 0)
            {
                // A toggled off mask no longer covers the mouth.
                if (TryComp<MaskComponent>(worn, out var mask) && mask.IsToggled)
                    continue;

                return;
            }

            if (HasComp<BreathToolComponent>(worn))
                return;
        }

        RaiseNetworkEvent(new ColdBreathParticleEvent(_transform.GetMapCoordinates(ent.Owner), GetNetEntity(ent.Owner)), Filter.Pvs(ent.Owner));
    }
}
