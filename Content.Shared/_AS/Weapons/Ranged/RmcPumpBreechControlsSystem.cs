using Content.Shared._AS.CCVar;
using Content.Shared._RMC14.Weapons.Common;
using Content.Shared._RMC14.Weapons.Ranged;
using Content.Shared.Interaction.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.Weapons.Ranged;

/// <summary>
/// Lets players choose between using interact in-hand breech/pumping and RMC styled breech pumping
/// </summary>
public sealed partial class RmcPumpBreechControlsSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = null!;
    [Dependency] private INetManager _net = null!;
    [Dependency] private ISharedPlayerManager _player = null!;

    private readonly Dictionary<NetUserId, bool> _uniqueActionPreferences = new();
    private bool _localUniqueActionEnabled;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PumpActionComponent, UseInHandEvent>(OnUseInHand,
            before: [typeof(SharedGunSystem)],
            after: [typeof(SharedWieldableSystem)]); // make sure we can wield the gun
        SubscribeLocalEvent<BreechLoadedComponent, UseInHandEvent>(OnUseInHand,
            before: [typeof(SharedGunSystem)],
            after: [typeof(SharedWieldableSystem)]); // make sure we can wield the gun

        SubscribeLocalEvent<PumpActionComponent, CatchUniqueActionEvent>(OnCatchUniqueAction);
        SubscribeLocalEvent<BreechLoadedComponent, CatchUniqueActionEvent>(OnCatchUniqueAction);

        SubscribeNetworkEvent<RmcPumpBreechPreferenceEvent>(OnPreferenceChanged);
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnLocalPlayerAttached);

        if (_net.IsClient)
            Subs.CVar(_cfg, ASCCVars.RmcUniqueAction, OnPreferenceChanged, true);
    }

    private void OnUseInHand(Entity<PumpActionComponent> ent, ref UseInHandEvent args)
    {
        TryUniqueAction(ent.Owner, ref args);
    }

    private void OnUseInHand(Entity<BreechLoadedComponent> ent, ref UseInHandEvent args)
    {
        TryUniqueAction(ent.Owner, ref args);
    }

    // What we're doing is basically treating interact in-hand for pump/breech guns as if you were hitting the UniqueAction key
    private void TryUniqueAction(EntityUid target, ref UseInHandEvent args)
    {
        if (args.Handled || IsUniqueActionEnabled(args.User))
            return;

        var uniqueAction = new UniqueActionEvent(args.User);
        RaiseLocalEvent(target, uniqueAction);

        args.Handled = uniqueAction.Handled;
    }

    private void OnCatchUniqueAction(Entity<PumpActionComponent> ent, ref CatchUniqueActionEvent args)
    {
        BlockDisabledUniqueAction(args.UserUid, ref args);
    }

    private void OnCatchUniqueAction(Entity<BreechLoadedComponent> ent, ref CatchUniqueActionEvent args)
    {
        BlockDisabledUniqueAction(args.UserUid, ref args);
    }

    // Eat UniqueAction attempts for pump/breech guns when RMC style is toggled off
    private void BlockDisabledUniqueAction(EntityUid user, ref CatchUniqueActionEvent args)
    {
        if (!IsUniqueActionEnabled(user))
            args.Cancel();
    }

    private bool IsUniqueActionEnabled(EntityUid user)
    {
        return _net.IsClient ? _localUniqueActionEnabled
            : _player.TryGetSessionByEntity(user, out var session) &&
              _uniqueActionPreferences.GetValueOrDefault(session.UserId);
    }

    private void OnPreferenceChanged(bool enabled)
    {
        _localUniqueActionEnabled = enabled;
        SendPreference();
    }

    private void OnLocalPlayerAttached(LocalPlayerAttachedEvent args)
    {
        SendPreference();
    }

    private void SendPreference()
    {
        if (_net.IsClient && _player.LocalSession != null)
            RaiseNetworkEvent(new RmcPumpBreechPreferenceEvent(_localUniqueActionEnabled));
    }

    private void OnPreferenceChanged(RmcPumpBreechPreferenceEvent args, EntitySessionEventArgs sessionArgs)
    {
        if (_net.IsServer)
            _uniqueActionPreferences[sessionArgs.SenderSession.UserId] = args.Enabled;
    }
}

// Synchronizes the pump/breech rmc preference for input validation.
[Serializable, NetSerializable]
public sealed class RmcPumpBreechPreferenceEvent(bool enabled) : EntityEventArgs
{
    public bool Enabled { get; } = enabled;
}
