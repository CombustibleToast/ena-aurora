using System.Numerics;
using Content.Shared._AS.AFK;
using Content.Shared._AS.CCVar;
using Robust.Client.Input;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client._AS.AFK;

/// <summary>
/// Tracks AFK status for the local player and notifies the server when it changes.
/// </summary>
public sealed partial class AfkSsdSystem : EntitySystem
{
    // I sat here trying to figure out why the hell it was so inconsistent until I realized it was counting the
    // fucking micromovements from the mouse so now we have a threshold for how far the mouse has to move before it counts as activity.
    private const float MouseActivityDistanceSquared = 10f * 10f;

    [Dependency] private IConfigurationManager _cfg = null!;
    [Dependency] private IGameTiming _timing = null!;
    [Dependency] private IInputManager _input = null!;

    private TimeSpan _lastActivity;
    private Vector2 _lastMousePosition;
    private int _timeoutSeconds;
    private bool _reportedInactive;

    public override void Initialize()
    {
        base.Initialize();

        _lastActivity = _timing.RealTime;
        _lastMousePosition = _input.MouseScreenPosition.Position;
        Subs.CVar(_cfg, ASCCVars.InactivitySsdTime, SetTimeout, true);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        _input.FirstChanceOnKeyEvent += OnKeyEvent;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _input.FirstChanceOnKeyEvent -= OnKeyEvent;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var mousePosition = _input.MouseScreenPosition.Position;
        if (Vector2.DistanceSquared(mousePosition, _lastMousePosition) >= MouseActivityDistanceSquared)
        {
            _lastMousePosition = mousePosition;
            SetActive();
        }

        if (_reportedInactive || _timeoutSeconds <= 0)
            return;

        if (_timing.RealTime - _lastActivity < TimeSpan.FromSeconds(_timeoutSeconds))
            return;

        _reportedInactive = true;
        RaiseNetworkEvent(new ClientInactivityChangedEvent(true));
    }

    private void OnKeyEvent(KeyEventArgs args, KeyEventType type)
    {
        SetActive();
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        SetActive();
    }

    private void SetActive()
    {
        _lastActivity = _timing.RealTime;

        if (!_reportedInactive)
            return;

        _reportedInactive = false;
        RaiseNetworkEvent(new ClientInactivityChangedEvent(false));
    }

    private void SetTimeout(int value)
    {
        _timeoutSeconds = value;

        if (value > 0)
            return;

        _lastActivity = _timing.RealTime;
        _reportedInactive = false;
    }
}
