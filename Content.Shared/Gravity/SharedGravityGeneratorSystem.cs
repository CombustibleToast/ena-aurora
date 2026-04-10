using Content.Shared.Popups;
using Content.Shared.Construction.Components;
using Content.Shared.Emp; // Frontier: Upstream - #28984

namespace Content.Shared.Gravity;

public abstract class SharedGravityGeneratorSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GravityGeneratorComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);

        SubscribeLocalEvent<GravityGeneratorComponent, EmpPulseEvent>(OnEmpPulse); // Frontier: Upstream - #28984
    }

    /// <summary>
    /// Prevent unanchoring when gravity is active
    /// </summary>
    private void OnUnanchorAttempt(Entity<GravityGeneratorComponent> ent, ref UnanchorAttemptEvent args)
    {
        if (!ent.Comp.GravityActive)
            return;

        _popupSystem.PopupClient(Loc.GetString("gravity-generator-unanchoring-failed"), ent.Owner, args.User, PopupType.Medium);

        args.Cancel();
    }

    private void OnEmpPulse(EntityUid uid, GravityGeneratorComponent component, EmpPulseEvent args) // Frontier: Upstream - #28984
    {
        /// i really don't think that the gravity generator should use normalised 0-1 charge
        /// as opposed to watts charge that every other battery uses

        if (!TryComp<ApcPowerReceiverComponent>(uid, out var powerReceiver))
            return;

        var ent = (uid, component, powerReceiver);

        // convert from normalised energy to watts and subtract
        float maxEnergy = component.ActivePowerUse / component.ChargeRate;
        float currentEnergy = maxEnergy * component.Charge;
        currentEnergy = Math.Max(0, currentEnergy - args.EnergyConsumption);

        // apply renormalised energy to charge variable
        component.Charge = currentEnergy / maxEnergy;

        // update power state
        UpdateState(ent);
    }
}
