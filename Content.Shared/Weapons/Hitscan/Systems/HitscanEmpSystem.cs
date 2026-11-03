using Content.Shared.Emp;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;

namespace Content.Shared.Weapons.Hitscan.Systems;

public sealed partial class HitscanEmpSystem : EntitySystem
{
    [Dependency] private SharedEmpSystem _emp = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HitscanEmpComponent, HitscanRaycastFiredEvent>(OnHitscanHit);
    }

    private void OnHitscanHit(Entity<HitscanEmpComponent> hitscan, ref HitscanRaycastFiredEvent args)
    {
        if (args.Data.HitEntity == null)
            return;

        _emp.EmpPulse(Transform(args.Data.HitEntity.Value).Coordinates, hitscan.Comp.Range, hitscan.Comp.EnergyConsumption, hitscan.Comp.DisableDuration);
    }
}