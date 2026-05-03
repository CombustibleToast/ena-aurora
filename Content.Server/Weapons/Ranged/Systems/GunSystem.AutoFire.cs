using Content.Shared.Damage.Components;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Map;
using Content.Server.Power.Components; // Frontier
using Content.Server.Power.EntitySystems; // Frontier
using Content.Shared.Interaction; // Frontier
using Content.Shared.Examine; // Frontier
using Content.Server.Popups; // Frontier
using Content.Shared.Power; // Frontier

namespace Content.Server.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    [Dependency] public PopupSystem _popup = default!; // Frontier
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        /*
         * On server because client doesn't want to predict other's guns.
         */

        // Automatic firing without stopping if the AutoShootGunComponent component is exist and enabled
        var query = EntityQueryEnumerator<GunComponent>();

        while (query.MoveNext(out var uid, out var gun))
        {
            if (gun.NextFire > Timing.CurTime)
                continue;

            if (TryComp(uid, out AutoShootGunComponent? autoShoot))
            {
                if (!autoShoot.Enabled)
                    continue;

                AttemptShoot((uid, gun));
            }
            else if (gun.BurstActivated)
            {
                var parent = TransformSystem.GetParentUid(uid);
                if (HasComp<DamageableComponent>(parent))
                    AttemptShoot(parent, (uid, gun), gun.ShootCoordinates ?? new EntityCoordinates(uid, gun.DefaultDirection));
                else
                    AttemptShoot((uid, gun));
            }
        }
    }

}
