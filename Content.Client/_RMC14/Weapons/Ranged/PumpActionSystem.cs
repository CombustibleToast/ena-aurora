using Content.Shared._RMC14.Input;
using Content.Shared._RMC14.Weapons.Ranged;
using Content.Shared._AS.CCVar; // Aurora's Song - RMC styled pump/breech ccvar
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Client.Input;
using Robust.Shared.Configuration; // Aurora's Song - RMC styled pump/breech ccvar

namespace Content.Client._RMC14.Weapons.Ranged;

public sealed partial class PumpActionSystem : SharedPumpActionSystem
{
    [Dependency] private IInputManager _input = default!;
    [Dependency] private IConfigurationManager _cfg = default!; // Aurora's Song - RMC styled pump/breech ccvar
    [Dependency] private SharedPopupSystem _popup = default!;

    protected override void OnExamined(Entity<PumpActionComponent> ent, ref ExaminedEvent args)
    {
        // Aurora's Song Start - Check if the player is using RMC styled breech/pump
        if (_cfg.GetCVar(ASCCVars.RmcUniqueAction) &&
            _input.TryGetKeyBinding(CMKeyFunctions.CMUniqueAction, out var bind))
            args.PushMarkup(Loc.GetString($"{ent.Comp.Examine}-with", ("key", bind.GetKeyString())), 1);
        else
            args.PushMarkup(Loc.GetString(ent.Comp.Examine), 1);
        // Aurora's Song End
    }

    protected override void OnAttemptShoot(Entity<PumpActionComponent> ent, ref AttemptShootEvent args)
    {
        if (args.Cancelled)
            return;

        base.OnAttemptShoot(ent, ref args);

        if (!ent.Comp.Pumped && TryComp<GunComponent>(ent.Owner, out var gun) && !gun.BurstActivated)
        {
            // Aurora's Song Start - Pick the correct FTL depending if the player is using the RmcUniqueAction
            var message = _cfg.GetCVar(ASCCVars.RmcUniqueAction) && _input.TryGetKeyBinding(CMKeyFunctions.CMUniqueAction, out var bind)
                ? Loc.GetString(ent.Comp.PopupKey, ("key", bind.GetKeyString()))
                : Loc.GetString(ent.Comp.Popup);
            _popup.PopupClient(message, args.User, args.User);
            // Aurora's Song End
        }
    }
}
