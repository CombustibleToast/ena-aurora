using Content.Client.Administration.UI.BanList.RoleBans;
using Content.Shared.Administration.BanList;

namespace Content.Client.Administration.UI.BanList;

/// <summary>
/// Radio Ban UI
/// </summary>
public sealed partial class BanListEui
{
    private RoleBanListControl RadioBanControl { get; set; } = default!;

    private void InitializeRadioBanList()
    {
        RadioBanControl = BanWindow.RadioBanList;
        RadioBanControl.LineIdsClicked += OnLineIdsClicked;
        RadioBanControl.SetScopeHeader(Loc.GetString("ban-list-header-channels"));
    }

    private void SetRadioBans(List<SharedBan> bans)
    {
        RadioBanControl.SetRoleBans(bans);
    }
}
