using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Interaction; // Frontier
using Content.Shared.Examine; // Frontier
using Content.Shared.Power; // Aurora - For Frontier addition of PowerChangedEvent
using Content.Shared.Power.EntitySystems; // Aurora - Move AutoFire to shared
using Content.Shared.Power.Components; // Aurora - Move AutoFire to shared
using Content.Shared.Popups; // Aurora - Move AutoFire to shared

namespace Content.Shared.Weapons.Ranged.Systems;

public partial class SharedGunSystem
{
    /* Aurora add start
     * Move these out of GunSystem.AutoFire.cs
     */
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    private readonly SharedPopupSystem _popup = default!;
    //Another [Dependency] field of type 'Content.Shared.Popups.SharedPopupSystem' already exists in this type with field 'PopupSystem'

    protected virtual void InitializeAutoFire()
    {
        SubscribeLocalEvent<AutoShootGunComponent, ActivateInWorldEvent>(OnActivateGun); // Frontier
        SubscribeLocalEvent<AutoShootGunComponent, ComponentInit>(OnGunInit); // Frontier
        SubscribeLocalEvent<AutoShootGunComponent, ComponentShutdown>(OnGunShutdown); // Frontier
        SubscribeLocalEvent<AutoShootGunComponent, ExaminedEvent>(OnGunExamine); // Frontier
        SubscribeLocalEvent<AutoShootGunComponent, PowerChangedEvent>(OnPowerChange); // Frontier
        SubscribeLocalEvent<AutoShootGunComponent, AnchorStateChangedEvent>(OnAnchorChange); // Frontier
    }

    // New Frontiers - Shuttle Gun Power Draw - makes shuttle guns require power if they
    // have an SharedApcPowerReceiverComponent
    // This code is licensed under AGPLv3. See AGPLv3.txt
    // Aurora - Moved out of GunSystem to SharedGunSystem
    private void OnGunExamine(EntityUid uid, AutoShootGunComponent component, ExaminedEvent args)
    {
        // Powered is already handled by other power components
        var enabled = Loc.GetString(component.On ? "gun-comp-enabled" : "gun-comp-disabled");

        args.PushMarkup(enabled);
    }

    private void OnActivateGun(EntityUid uid, AutoShootGunComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        component.On ^= true;

        if (!component.On)
        {
            _power.SetLoad(uid, 1); // Aurora - Move AutoFire to shared

            DisableGun(uid, component);
            args.Handled = true;
            _popup.PopupEntity(Loc.GetString("auto-fire-disabled"), uid, args.User);
        }
        else if (CanEnable(uid, component))
        {
            _power.SetLoad(uid, component.OriginalLoad); // Aurora - Move AutoFire to shared

            EnableGun(uid, component);
            args.Handled = true;
            _popup.PopupEntity(Loc.GetString("auto-fire-enabled"), uid, args.User);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("auto-fire-enabled-no-power"), uid, args.User);
        }
    }

    /// <summary>
    /// Tries to disable the AutoShootGun.
    /// </summary>
    public void DisableGun(EntityUid uid, AutoShootGunComponent component)
    {
        if (component.CanFire)
            component.CanFire = false;
    }

    public bool CanEnable(EntityUid uid, AutoShootGunComponent component)
    {
        var xform = Transform(uid);

        // Must be anchored to fire.
        if (!xform.Anchored)
            return false;

        // No power needed? Always works.
        SharedApcPowerReceiverComponent? apcPower = null; // Aurora - Move AutoFire to shared
        if (!_power.ResolveApc(uid, ref apcPower)) // Aurora - Move AutoFire to shared
            return true;

        // Not switched on? Won't work.
        if (!component.On)
            return false;

        return _power.IsPowered(uid);
    }

    public void EnableGun(EntityUid uid, AutoShootGunComponent component, TransformComponent? xform = null)
    {
        if (!component.CanFire)
            component.CanFire = true;
    }

    private void OnAnchorChange(EntityUid uid, AutoShootGunComponent component, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored && CanEnable(uid, component))
            EnableGun(uid, component);
        else
            DisableGun(uid, component);
    }

    private void OnGunInit(EntityUid uid, AutoShootGunComponent component, ComponentInit args)
    {
        SharedApcPowerReceiverComponent? apcPower = null; // Aurora - Move AutoFire to shared
        if (_power.ResolveApc(uid, ref apcPower) && component.OriginalLoad == 0) // Aurora - Move AutoFire to shared
            component.OriginalLoad = apcPower.Load;

        if (!component.On)
            return;

        if (CanEnable(uid, component))
            EnableGun(uid, component);
    }

    private void OnGunShutdown(EntityUid uid, AutoShootGunComponent component, ComponentShutdown args)
    {
        DisableGun(uid, component);
    }

    private void OnPowerChange(EntityUid uid, AutoShootGunComponent component, ref PowerChangedEvent args)
    {
        if (args.Powered && CanEnable(uid, component))
            EnableGun(uid, component);
        else
            DisableGun(uid, component);
    }
    // End of modified code
}
