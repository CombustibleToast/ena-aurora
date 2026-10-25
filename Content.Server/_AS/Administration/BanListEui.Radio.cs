using Content.Shared.Administration.BanList;
using Content.Shared.Database;
using Robust.Shared.Network;
using System.Threading.Tasks;

namespace Content.Server.Administration.BanList;

/// <summary>
/// Radio ban data
/// </summary>
public sealed partial class BanListEui
{
    private List<SharedBan> RadioBans { get; } = new();

    private Task LoadRadioBans(NetUserId userId)
    {
        return LoadBansCore(userId, BanType.Radio, RadioBans);
    }

    private void ClearRadioBans()
    {
        RadioBans.Clear();
    }
}
