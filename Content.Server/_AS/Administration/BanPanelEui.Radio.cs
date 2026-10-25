using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using Robust.Shared.Network;

namespace Content.Server.Administration;

/// <summary>
/// Adds radio bans to upstream ban UI
/// </summary>
public sealed partial class BanPanelEui
{
    private bool CreateRadioBans(Ban ban, NetUserId? targetUid)
    {
        if (targetUid == null)
        {
            _chat.DispatchServerMessage(Player, Loc.GetString("ban-panel-radio-player-required"));
            return true;
        }

        var radioBan = new CreateRadioBanInfo(ban.Reason);
        foreach (var channel in ban.RadioChannels ?? [])
            radioBan.AddChannel(channel);

        radioBan.WithBanningAdmin(Player.UserId);
        radioBan.WithSeverity(ban.Severity);
        radioBan.AddUser(targetUid.Value, ban.Target!);
        if (ban.BanDurationMinutes > 0)
            radioBan.WithMinutes(ban.BanDurationMinutes);
        _banManager.CreateRadioBan(radioBan);

        Close();
        return true;
    }
}
