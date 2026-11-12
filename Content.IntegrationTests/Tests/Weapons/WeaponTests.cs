using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Weapons;

public sealed class WeaponTests : InteractionTest
{
    protected override string PlayerPrototype => "MobHuman"; // The default test mob only has one hand
    private static readonly EntProtoId MobHuman = "MobHuman";
    private static readonly EntProtoId testWeapon = "NFWeaponRifleRepeater"; // Aurora's Song | changed from Mosin to repeater rifle, as new AS mosin functions differently and would cause a test failure.

    [Test]
    public async Task GunRequiresWieldTest()
    {
        var gunSystem = SEntMan.System<SharedGunSystem>();
        var damageSystem = SEntMan.System<DamageableSystem>();

        await AddAtmosphere(); // prevent the Urist from suffocating

        var urist = await SpawnTarget(MobHuman);
        var damageComp = Comp<DamageableComponent>(urist);

        var weapNet = await PlaceInHands(testWeapon); // Aurora's Song | mosinNet < weapNet
        var weapEnt = ToServer(weapNet); // Aurora's Song | mosinEnt < weapEnt

        await Pair.RunSeconds(2f); // Guns have a cooldown when picking them up.

        Assert.That(HasComp<GunRequiresWieldComponent>(weapNet), // Aurora's Song | mosinNet < weapNet
            "Looks like you've removed the 'GunRequiresWield' component from the NFWeaponRifleRepeater." + // Aurora's Song | Change this if you change the test weapon
            "If this was intentional, please update WeaponTests.cs to reflect this change!");

        var startAmmo = gunSystem.GetAmmoCount(weapEnt); // Aurora's Song | mosinEnt < weapEnt
        var wieldComp = Comp<WieldableComponent>(weapNet); // Aurora's Song | mosinNet < weapNet

        Assert.That(startAmmo, Is.GreaterThan(0), "Weapon was spawned with no ammo!"); // Aurora's Song | "Mosin was spawned" < "Weapon was spawned"
        Assert.That(wieldComp.Wielded, Is.False, "Weapon was spawned wielded!"); // Aurora's Song | "Mosin was spawned" < "Weapon was spawned"

        await AttemptShoot(urist, false); // should fail due to not being wielded
        var updatedAmmo = gunSystem.GetAmmoCount(weapEnt); // Aurora's Song | made test more generic after changing test weapon

        Assert.That(updatedAmmo,
            Is.EqualTo(startAmmo),
            "Weapon discharged ammo when the weapon should not have fired!"); // Aurora's Song | "Mosin discharged" < "Weapon discharged"
        Assert.That(damageSystem.GetTotalDamage(ToServer(urist)),
            Is.EqualTo(FixedPoint2.Zero),
            "Urist took damage when the weapon should not have fired!");

        await UseInHand();

        Assert.That(wieldComp.Wielded, Is.True, "Weapon failed to wield when interacted with!"); // Aurora's Song | "Mosin failed" < "Weapon failed"

        await AttemptShoot(urist);
        updatedAmmo = gunSystem.GetAmmoCount(weapEnt); // Aurora's Song | mosinEnt < weapEnt

        Assert.That(updatedAmmo, Is.EqualTo(startAmmo - 1), "Weapon failed to discharge appropriate amount of ammo!"); // Aurora's Song | "Mosin failed" < "Weapon failed"
        Assert.That(damageSystem.GetTotalDamage(ToServer(urist)),
            Is.GreaterThan(FixedPoint2.Zero),
            "Weapon was fired but urist sustained no damage!"); // Aurora's Song | "Mosin was fired" < "Weapon was fired"
    }
}
