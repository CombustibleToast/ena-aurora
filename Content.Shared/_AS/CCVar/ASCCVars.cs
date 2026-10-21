using Robust.Shared.Configuration;

namespace Content.Shared._AS.CCVar;

/// <summary>
/// Contains CVars used by Aurora Song.
/// </summary>
[CVarDefs]
public sealed class ASCCVars
{
    /// <summary>
    /// How many seconds a connected player can be inactive before they are treated as SSD.
    /// Set to zero or less to disable automatic SSD.
    /// Defaults to 20 minutes.
    /// </summary>
    public static readonly CVarDef<int> InactivitySsdTime =
        CVarDef.Create("as.afk.ssd_time", 1200, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Disables scopophobia-triggering visual effects.
    /// </summary>
    public static readonly CVarDef<bool> Scopophobia =
        CVarDef.Create("accessibility.scopophobia", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// The number of Cargo Depots to spawn in every round
    /// </summary>
    public static readonly CVarDef<int> Motels =
        CVarDef.Create("as.worldgen.motels", 2, CVar.SERVERONLY);

    /// <summary>
    /// Should admin alerts give TP and Coordinate links
    /// </summary>
    public static readonly CVarDef<bool> ShowAdminLinks = CVarDef.Create("as.adminlogs.show_admin_links", false, CVar.SERVERONLY);

    // Opt-in for RMC dedicated unique action pump/breech keybinds.
    public static readonly CVarDef<bool> RmcUniqueAction =
        CVarDef.Create("as.control.rmc_unique_action", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
