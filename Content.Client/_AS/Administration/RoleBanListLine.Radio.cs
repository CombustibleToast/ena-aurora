using System.Linq;
using Content.Shared.Administration.BanList;
using Content.Shared.Database;

namespace Content.Client.Administration.UI.BanList.RoleBans;

/// <summary>
/// Formatting for radio bans
/// </summary>
public sealed partial class RoleBanListLine
{
    private static string FormatScope(SharedBan ban)
    {
        return ban.Type == BanType.Radio ? string.Join(", ", (ban.Roles ?? []).Select(role => role.RoleId)) : string.Join(", ", ban.Roles ?? []);
    }
}
