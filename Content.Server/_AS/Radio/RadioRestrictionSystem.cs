using System.Globalization;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Radio;
using Robust.Shared.Player;

namespace Content.Server._AS.Radio;

/// <summary>
/// Prevents a player with a radio restriction (usually a ban) from talking over a channel
/// </summary>
public sealed partial class RadioRestrictionSystem : EntitySystem
{
    [Dependency] private ISharedPlayerManager _players = default!;
    [Dependency] private IBanManager _banManager = default!;
    [Dependency] private IChatManager _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadioSendAttemptEvent>(OnRadioSendAttempt);
    }

    private void OnRadioSendAttempt(ref RadioSendAttemptEvent args)
    {
        if (args.MessageSource is not { } messageSource ||
            !_players.TryGetSessionByEntity(messageSource, out var session) ||
            _banManager is not IRadioBanManager radioBans ||
            !radioBans.TryGetRadioBanExpiration(session, args.Channel.ID, out var expires))
            return;

        args.Cancelled = true;
        var feedback = expires is { } expiration
            ? Loc.GetString("radio-ban-transmission-blocked-until", ("channel", args.Channel.LocalizedName), ("expires", expiration.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture)))
            : Loc.GetString("radio-ban-transmission-blocked", ("channel", args.Channel.LocalizedName));
        _chat.DispatchServerMessage(session, feedback, suppressLog: true);
    }
}
