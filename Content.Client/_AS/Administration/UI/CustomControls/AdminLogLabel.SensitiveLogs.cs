using Content.Shared.Administration.Logs;

namespace Content.Client.Administration.UI.CustomControls;

public sealed partial class AdminLogLabel
{
    public void Reveal(SharedAdminLog log)
    {
        Log = log;
        SetMessage($"{log.Date:HH:mm:ss}: {log.Message}");
    }
}
