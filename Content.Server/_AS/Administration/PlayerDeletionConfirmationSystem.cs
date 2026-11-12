using Content.Server.Popups;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Robust.Server.Placement;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Network.Messages;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._AS.Administration;

public sealed partial class PlayerDeletionConfirmationSystem : EntitySystem
{
    private static readonly TimeSpan ConfirmationTime = TimeSpan.FromSeconds(2);

    [Dependency] private IGameTiming _timing = null!;
    [Dependency] private IPlacementManager _placement = null!;
    [Dependency] private IPlayerManager _players = null!;
    [Dependency] private PopupSystem _popup = null!;

    private readonly Dictionary<NetUserId, PendingDeletion> _pendingDeletions = new();
    private Func<MsgPlacement, bool>? _oldPlacementCheck;
    private Func<MsgPlacement, bool> _playerDeleteCheck = null!;

    public override void Initialize()
    {
        base.Initialize();
        _playerDeleteCheck = CheckPlayerDelete;
        HookPlacementCheck();

        // This looks scary, but Sandbox also sets this during init, so ours needs to go last.
        Timer.Spawn(0, () =>
        {
            if (_placement.AllowPlacementFunc != _playerDeleteCheck)
                HookPlacementCheck();
        });
    }

    public override void Shutdown()
    {
        if (_placement.AllowPlacementFunc == _playerDeleteCheck)
            _placement.AllowPlacementFunc = _oldPlacementCheck;

        base.Shutdown();
    }

    private void HookPlacementCheck()
    {
        _oldPlacementCheck = _placement.AllowPlacementFunc;
        _placement.AllowPlacementFunc = _playerDeleteCheck;
    }

    private bool CheckPlayerDelete(MsgPlacement message)
    {
        if (_oldPlacementCheck != null && !_oldPlacementCheck(message))
            return false;

        var userId = message.MsgChannel.UserId;
        if (message.PlaceType != PlacementManagerMessage.RequestEntRemove ||
            !TryGetEntity(message.EntityUid, out var target) ||
            !IsPlayer(target.Value) || _pendingDeletions.TryGetValue(userId, out var pending) &&
            pending.Target == target.Value &&
            _timing.RealTime - pending.Time <= ConfirmationTime)
        {
            _pendingDeletions.Remove(userId);
            return true;
        }

        _pendingDeletions[userId] = new PendingDeletion(target.Value, _timing.RealTime);

        var session = _players.GetSessionByChannel(message.MsgChannel);
        _popup.PopupCursor(Loc.GetString("entity-player-delete-confirm"), session, PopupType.LargeCaution);

        return false;
    }

    private bool IsPlayer(EntityUid target)
    {
        if (HasComp<ActorComponent>(target))
            return true;

        return TryComp<MindContainerComponent>(target, out var container) && container.Mind is { } mindId && TryComp<MindComponent>(mindId, out var mind) && (mind.UserId != null || mind.OriginalOwnerUserId != null);
    }

    private readonly record struct PendingDeletion(EntityUid Target, TimeSpan Time);
}
