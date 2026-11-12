using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration.BanList;

[Serializable, NetSerializable]
public sealed class BanListEuiState : EuiStateBase
{
    public BanListEuiState(string banListPlayerName, List<SharedBan> bans, List<SharedBan> roleBans, List<SharedBan> radioBans) // Aurora's Song - Radio Bans
    {
        BanListPlayerName = banListPlayerName;
        Bans = bans;
        RoleBans = roleBans;
        RadioBans = radioBans; // Aurora's Song
    }

    public string BanListPlayerName { get; }
    public List<SharedBan> Bans { get; }
    public List<SharedBan> RoleBans { get; }
    public List<SharedBan> RadioBans { get; } // Aurora's Song
}
