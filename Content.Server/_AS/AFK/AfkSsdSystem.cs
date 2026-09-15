using Content.Server.Afk;
using Content.Shared._AS.AFK;
using Content.Shared._AS.CCVar;
using Content.Shared.SSDIndicator;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server._AS.AFK;

/// <summary>
/// Treats players who have had not input for the set time as SSD.
/// </summary>
public sealed partial class AfkSsdSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = null!;
    [Dependency] private IAfkManager _afk = null!;
    [Dependency] private SSDIndicatorSystem _ssd = null!;

    private readonly HashSet<ICommonSession> _automaticallySsd = [];
    private int _timeoutSeconds;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, ASCCVars.InactivitySsdTime, SetTimeout, true);
        SubscribeNetworkEvent<ClientInactivityChangedEvent>(OnClientInactivityChanged);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _automaticallySsd.Clear();
    }

    private void OnClientInactivityChanged(ClientInactivityChangedEvent message, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;

        if (!message.Inactive)
        {
            SetActive(session);
            return;
        }

        if (_timeoutSeconds <= 0 || session.Status != SessionStatus.InGame || session.AttachedEntity is not { } entity || !_afk.IsInactiveFor(session, TimeSpan.FromSeconds(_timeoutSeconds)))
            return;

        _ssd.SetSsdState(entity, true);
        _automaticallySsd.Add(session);
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        SetActive(args.Player);
    }

    private void OnPlayerDetached(PlayerDetachedEvent args)
    {
        _automaticallySsd.Remove(args.Player);
    }

    private void SetActive(ICommonSession session)
    {
        // If the player is not connected, we don't care about them being active or not.
        if (session.Status != SessionStatus.InGame)
            return;

        if (_automaticallySsd.Remove(session) && session.AttachedEntity is { } entity)
            _ssd.SetSsdState(entity, false);
    }

    private void SetTimeout(int value)
    {
        _timeoutSeconds = value;

        if (value > 0)
            return;

        foreach (var session in _automaticallySsd)
        {
            if (session is { Status: SessionStatus.InGame, AttachedEntity: { } entity })
                _ssd.SetSsdState(entity, false);
        }

        _automaticallySsd.Clear();
    }
}
