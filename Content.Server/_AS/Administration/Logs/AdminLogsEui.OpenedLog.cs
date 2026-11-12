using Content.Shared.Database;

namespace Content.Server.Administration.Logs;

/// <summary>
/// Logs whenever administrator opens the admin logs menu.
/// </summary>
public sealed partial class AdminLogsEui
{
    private void AuditLogsOpened()
    {
        _adminLogs.Add(LogType.Action,
            LogImpact.Low,
            $"{Player:actor} opened the admin logs menu");
    }
}
